import {bookingsService} from "../../services/bookingsService.js"

const LATE_CANCELLATION_HOURS_THRESHOLD = 48;
const LATE_CANCELLATION_RATE = 0.20;

class CheckInPage extends HTMLElement {
    constructor() {
        super();
        this.checkList=[];
        this.confirmDialog = null;
    }

    connectedCallback() {
        this.render();
    }

    async render(){
        const html = await fetch(new URL("./check-in.html", import.meta.url)).then(response => response.text());
        const template = document.createElement("template");
        template.innerHTML = html;
        this.appendChild(template.content.cloneNode(true));

        const css = await fetch(new URL("./check-in.css", import.meta.url)).then(response => response.text());
        const style = document.createElement("style");
        style.textContent = css;
        this.appendChild(style);

        this.confirmDialog = this.ensureConfirmDialog();

        const listElement = this.querySelector(".check-in__list");
        listElement?.addEventListener("click", (event) => this.onListClick(event));

        await this.loadChecks();
    }

    async onListClick(event) {
        const actionButton = event.target.closest(
            ".check-in__btn-check-in, .check-in__btn-check-out, .check-in__btn-cancel"
        );

        if (!actionButton || !this.contains(actionButton)) {
            return;
        }

        const bookingId = actionButton.dataset.id;
        if (!bookingId) {
            return;
        }

        const selectedBooking = this.checkList.find((booking) => booking.id === Number(bookingId));
        const guestLabel = selectedBooking?.mainGuestFullName || `reserva #${bookingId}`;

        let confirmation = null;
        let action = null;

        if (actionButton.classList.contains("check-in__btn-check-in")) {
            confirmation = {
                title: "Confirmar check-in",
                message: `Estas seguro de registrar el check-in para ${guestLabel}?`,
                confirmText: "Aceptar",
                cancelText: "Cancelar",
            };
            action = () => this.checkIn(bookingId);
        } else if (actionButton.classList.contains("check-in__btn-check-out")) {
            confirmation = {
                title: "Confirmar check-out",
                message: `Estas seguro de registrar el check-out para ${guestLabel}?`,
                confirmText: "Aceptar",
                cancelText: "Cancelar",
            };
            action = () => this.checkOut(bookingId);
        } else if (actionButton.classList.contains("check-in__btn-cancel")) {
            const feePreview = this.calculateCancellationFeePreview(selectedBooking);
            const feeMessage = feePreview.applies
                ? `Se cobrara una comision de ${this.formatMoney(feePreview.fee)} (20% de ${this.formatMoney(feePreview.referencePrice)}).`
                : "No se cobrara comision (faltan 48 horas o mas para el check-in).";

            confirmation = {
                title: "Confirmar cancelacion",
                message: `Estas seguro de cancelar la reserva de ${guestLabel}? ${feeMessage}`,
                confirmText: "Aceptar",
                cancelText: "Cancelar",
                variant: "danger",
            };
            action = () => this.cancel(bookingId);
        }

        if (!confirmation || !action || !this.confirmDialog) {
            return;
        }

        const accepted = await this.confirmDialog.open(confirmation);
        if (!accepted) {
            return;
        }

        try {
            await action();
        } catch (error) {
            console.error("Error al ejecutar accion de reserva:", error);
        }

        await this.loadChecks();
    }

    ensureConfirmDialog() {
        let dialog = this.querySelector("confirm-dialog");
        if (dialog) {
            return dialog;
        }

        dialog = document.createElement("confirm-dialog");
        this.appendChild(dialog);
        return dialog;
    }

    async loadChecks() {
        const listElement = this.querySelector(".check-in__list");
        const check = await bookingsService.getAgenda();
        this.checkList = Array.isArray(check) ? check : [];
        this.renderChecks(this.checkList, listElement);
    }
    
    renderChecks(checks, listElement){
        if (!listElement) {
            return;
        }

        listElement.innerHTML = "";

        if(!checks || checks.length === 0){
            listElement.innerHTML = `
                <not-found-component text="No hay reservas disponibles."></not-found-component>
            `;
            return;
        }

        const fragment = document.createDocumentFragment();
        checks.forEach(booking => {
            const bookingElement = document.createElement("div");
            bookingElement.classList.add("check-in__item");

            const status = booking.status.toLowerCase();
            const classButton = status === "confirmed" ? "check-in__btn-check-in" : "check-in__btn-check-out";
            const buttonText = status === "confirmed" ? "Check In" : "Check Out";
            const hiddenButton = status === "checkedin" ? "hidden" : "";

            let statusText ="";
            if (booking.status.toLowerCase() === "confirmed") {
                statusText = "Confirmada";
            }else if (booking.status.toLowerCase() === "cancelled") {
                statusText = "Cancelada";
            }else if (booking.status.toLowerCase() === "checkedin") {
                statusText = "Checked In";
            }else if (booking.status.toLowerCase() === "checkedout") {
                statusText = "Checked Out";
            }else {
                statusText = "Desconocido";
            }
            bookingElement.innerHTML = `
                <div class="check-in__item-header">
                    <h2 class="check-in__guest-name">${booking.mainGuestFullName}</h2>
                    <div class="check-in__status-container"> 
                        <p class="check-in__room-info">Habitacion ${booking.roomNumber} - ${booking.roomTypeName} - $${booking.roomTypePricePerNight.toFixed(2)} - Numero huespedes: ${booking.numberGuests}</p>
                        <p class="check-in__status-input">${statusText}</p>
                    </div>
                </div>
                
                <div class="check-in__item-data">
                    <p class="check-in__info">Ingreso: ${new Date(booking.checkInDate).toLocaleDateString()} - Salida: ${new Date(booking.checkOutDate).toLocaleDateString()}</p>
                </div>
                
                <div class="check-in__buttons">
                    <button type="button" data-id="${booking.id}" class="${classButton}">${buttonText}</button>
                    <button type="button" data-id="${booking.id}" class="check-in__btn-cancel" ${hiddenButton}>Cancelar</button>
                </div>
            `;
            fragment.appendChild(bookingElement);
        });
        listElement.appendChild(fragment);
    }
    checkIn(BookingId){
        const checkInPromise = bookingsService.checkIn(BookingId);

        return checkInPromise;
    }
    checkOut(BookingId){
        const checkOutPromise = bookingsService.checkOut(BookingId);
        return checkOutPromise;
    }
    cancel(BookingId){
        const cancelPromise = bookingsService.cancel(BookingId);
        return cancelPromise;
    }

    calculateCancellationFeePreview(booking) {
        const referencePrice = Number(booking?.roomTypePricePerNight ?? 0);
        const checkInDate = new Date(booking?.checkInDate);

        if (Number.isNaN(checkInDate.getTime()) || !Number.isFinite(referencePrice) || referencePrice <= 0) {
            return {
                applies: false,
                fee: 0,
                referencePrice: 0,
            };
        }

        const hoursBeforeCheckIn = (checkInDate.getTime() - Date.now()) / (1000 * 60 * 60);
        const applies = hoursBeforeCheckIn < LATE_CANCELLATION_HOURS_THRESHOLD;
        const fee = applies ? this.roundMoney(referencePrice * LATE_CANCELLATION_RATE) : 0;

        return {
            applies,
            fee,
            referencePrice,
        };
    }

    roundMoney(value) {
        return Math.round((Number(value) + Number.EPSILON) * 100) / 100;
    }

    formatMoney(value) {
        return `$${this.roundMoney(value).toFixed(2)}`;
    }
}

customElements.define("check-in-page", CheckInPage);