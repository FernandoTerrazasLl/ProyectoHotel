import {bookingsService} from "../../services/bookingsService.js"
import {formatCurrency, formatRoomInfo} from "../../utils/formatters.js"
import {LATE_CANCELLATION_HOURS_THRESHOLD, LATE_CANCELLATION_RATE} from "../../config.js"


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
                ? `Se cobrara una comision de ${formatCurrency(feePreview.fee)} (100% de 1 noche: ${formatCurrency(feePreview.referencePrice)}).`
                : "No se cobrara comision (faltan 24 horas o mas para el check-in).";

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

            // Smell 11 (Long Method): Extracted status text logic to getBookingStatusLabel helper method
            const statusText = this.getBookingStatusLabel(booking.status, booking.cancellationFee);

            bookingElement.innerHTML = `
                <div class="check-in__item-header">
                    <h2 class="check-in__guest-name">${booking.mainGuestFullName}</h2>
                    <div class="check-in__status-container"> 
                        <p class="check-in__room-info">${formatRoomInfo(booking)}</p>
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

    // Smell 11: Extracted helper to resolve cognitive complexity and long method smell
    getBookingStatusLabel(status, cancellationFee) {
        const normalizedStatus = status.toLowerCase();
        if (normalizedStatus === "confirmed") {
            return "Confirmada";
        }
        if (normalizedStatus === "cancelled") {
            let label = "Cancelada";
            if (cancellationFee !== undefined && cancellationFee !== null) {
                label += ` - Comisión cobrada: ${formatCurrency(cancellationFee)}`;
            }
            return label;
        }
        if (normalizedStatus === "checkedin") {
            return "Checked In";
        }
        if (normalizedStatus === "checkedout") {
            return "Checked Out";
        }
        return "Desconocido";
    }

    // Smell 4, 5, 6: Renamed C#-style parameter BookingId to bookingId
    checkIn(bookingId){
        const checkInPromise = bookingsService.checkIn(bookingId);
        return checkInPromise;
    }
    checkOut(bookingId){
        const checkOutPromise = bookingsService.checkOut(bookingId);
        return checkOutPromise;
    }
    cancel(bookingId){
        const cancelPromise = bookingsService.cancel(bookingId);
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
        const fee = applies ? (referencePrice * LATE_CANCELLATION_RATE) : 0;

        return {
            applies,
            fee,
            referencePrice,
        };
    }
}

customElements.define("check-in-page", CheckInPage);