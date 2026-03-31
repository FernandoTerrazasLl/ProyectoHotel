import { bookingsService } from "../../../services/bookingsService.js";

export const BookingListMixin = {
    async loadBookings() {
        const listElement = this.querySelector(".booking__list");
        const bookings = await bookingsService.getAgenda();
        this.bookingsList = Array.isArray(bookings) ? bookings : [];
        this.renderBookings(this.bookingsList, listElement);
    },

    renderBookings(bookings, listElement) {
        if (!listElement) {
            return;
        }

        listElement.innerHTML = "";

        if (!bookings || bookings.length === 0) {
            listElement.innerHTML = `
                <not-found-component text="No hay reservas disponibles."></not-found-component>
            `;
            return;
        }

        const fragment = document.createDocumentFragment();
        bookings.forEach((booking) => {
            const bookingElement = document.createElement("div");
            bookingElement.classList.add("booking__item");

            let status ="";
            if (booking.status.toLowerCase() === "confirmed") {
                status = "Confirmada";
            }else if (booking.status.toLowerCase() === "cancelled") {
                status = "Cancelada";
            }else if (booking.status.toLowerCase() === "checkedin") {
                status = "Checked In";
            }else if (booking.status.toLowerCase() === "checkedout") {
                status = "Checked Out";
            }else {
                status = "Desconocido";
            }

            bookingElement.innerHTML = `
                <div class="booking__item-header">
                    <h2 class="booking__guest-name">${booking.mainGuestFullName}</h2>
                    <div class="booking__status-container"> 
                        <p class="booking__room-info">Habitacion ${booking.roomNumber} - ${booking.roomTypeName} - $${booking.roomTypePricePerNight.toFixed(2)} - Numero huespedes: ${booking.numberGuests}</p>
                        <p class="booking__status-input">${status}</p>
                    </div>
                </div>
                
                <div class="booking__item-data">
                    <p class="booking__info">Ingreso: ${new Date(booking.checkInDate).toLocaleDateString()} - Salida: ${new Date(booking.checkOutDate).toLocaleDateString()}</p>
                </div>
                `;
            fragment.appendChild(bookingElement);
        });
        listElement.appendChild(fragment);
    },

    filterBookings(rawValue) {
        const value = rawValue.trim().toLowerCase();
        const listElement = this.querySelector(".booking__list");

        if (!value) {
            this.renderBookings(this.bookingsList, listElement);
            return;
        }

        const filteredBookings = this.bookingsList.filter((booking) => {
            const guestName = (booking.mainGuestFullName || "").toLowerCase();
            return guestName.includes(value);
        });

        this.renderBookings(filteredBookings, listElement);
    },
};