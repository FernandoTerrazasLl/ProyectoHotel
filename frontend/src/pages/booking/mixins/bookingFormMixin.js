import { roomTypesService } from "../../../services/roomTypesService.js";
import { roomsService } from "../../../services/roomsService.js";
import { guestsService } from "../../../services/guestsService.js";
import { bookingsService } from "../../../services/bookingsService.js";

export const BookingFormMixin = {
    form() {
        const createBtn = this.querySelector(".booking__create-btn");
        const formBackground = this.querySelector(".booking__form-background");
        const cancelBtn = this.querySelector(".booking__form-cancel");
        const formElement = this.querySelector(".booking__form");
        const roomTypeSelect = this.querySelector('select[name="roomType"]');
        const checkInInput = this.querySelector('input[name="checkInDate"]');
        const checkOutInput = this.querySelector('input[name="checkOutDate"]');

        if (!createBtn || !formBackground || !cancelBtn || !formElement || !roomTypeSelect || !checkInInput || !checkOutInput) {
            return;
        }

        const today = this.getTodayDateString();
        checkInInput.min = today;
        checkOutInput.min = today;

        createBtn.addEventListener("click", () => {
            this.hideFeedback();
            formBackground.classList.remove("booking__form-background--hidden");
            formElement.reset();
            this.availableRooms = [];
            this.resetRoomsOptions();
            checkInInput.min = today;
            checkOutInput.min = today;
        });

        cancelBtn.addEventListener("click", () => {
            this.hideFeedback();
            formBackground.classList.add("booking__form-background--hidden");
        });

        formElement.addEventListener("submit", async (event) => {
            event.preventDefault();

            const validationErrors = this.validateBookingForm(formElement);
            if (validationErrors.length > 0) {
                this.showFeedback(validationErrors, "error");
                return;
            }

            await this.createBooking(formElement, formBackground);
        });

        this.loadRoomTypeOptions();

        const loadRooms = () => {
            const roomTypeId = roomTypeSelect.value;
            const checkInDate = checkInInput.value;
            const checkOutDate = checkOutInput.value;
            this.loadAvailableRooms(roomTypeId, checkInDate, checkOutDate);
        };

        roomTypeSelect.addEventListener("change", loadRooms);
        checkInInput.addEventListener("change", () => {
            checkOutInput.min = checkInInput.value || today;
            loadRooms();
        });
        checkOutInput.addEventListener("change", loadRooms);

        this.loadGuests();
    },

    async loadRoomTypeOptions() {
        const roomSelect = this.querySelector('select[name="roomType"]');
        const roomTypes = await roomTypesService.getAll();
        if (!roomSelect) {
            return;
        }

        const fragment = document.createDocumentFragment();
        roomTypes.forEach((roomType) => {
            const option = document.createElement("option");
            option.value = roomType.id;
            option.textContent = `${roomType.name} - $${roomType.pricePerNight.toFixed(2)} - Capacidad: ${roomType.capacity}`;
            fragment.appendChild(option);
        });
        roomSelect.appendChild(fragment);
    },

    async loadAvailableRooms(roomTypeId, checkInDate, checkOutDate) {
        if (!roomTypeId || !checkInDate || !checkOutDate) {
            this.availableRooms = [];
            this.resetRoomsOptions();
            return;
        }

        const today = this.getTodayDateString();
        if (checkInDate < today || checkOutDate < today) {
            this.availableRooms = [];
            this.resetRoomsOptions();
            this.showFeedback("No se permiten fechas en el pasado.", "error");
            return;
        }

        if (new Date(checkOutDate) <= new Date(checkInDate)) {
            this.availableRooms = [];
            this.resetRoomsOptions();
            this.showFeedback("La fecha de salida debe ser posterior a la fecha de ingreso.", "error");
            return;
        }

        this.hideFeedback();
        this.resetRoomsOptions();

        const roomsRequest = await roomsService.getByAvailability(roomTypeId, checkInDate, checkOutDate);
        const rooms = Array.isArray(roomsRequest?.data) ? roomsRequest.data : [];
        this.availableRooms = rooms;

        const roomSelect = this.querySelector('select[name="room"]');
        if (!roomSelect) {
            return;
        }

        const fragment = document.createDocumentFragment();
        rooms.forEach((room) => {
            const option = document.createElement("option");
            option.value = room.id;
            option.textContent = `Habitacion ${room.roomNumber}`;
            fragment.appendChild(option);
        });
        roomSelect.appendChild(fragment);

        if (rooms.length === 0) {
            this.showFeedback("No hay habitaciones disponibles para esas fechas y tipo seleccionado.", "error");
        }
    },

    resetRoomsOptions() {
        const roomSelect = this.querySelector('select[name="room"]');
        if (!roomSelect) {
            return;
        }

        roomSelect.innerHTML = '<option value="">Seleccionar</option>';
    },

    buildBookingPayload(formElement) {
        const formData = new FormData(formElement);
        const selectedGuestIds = this.getSelectedGuestIds();

        if (selectedGuestIds.length === 0) {
            return null;
        }

        return {
            guestIds: selectedGuestIds,
            mainGuestId: selectedGuestIds[0],
            roomId: Number(formData.get("room")),
            checkInDate: String(formData.get("checkInDate") || ""),
            checkOutDate: String(formData.get("checkOutDate") || ""),
            numberGuests: selectedGuestIds.length,
        };
    },

    getSelectedGuestIds() {
        return Array.from(this.querySelector('select[name="guestIds"]')?.selectedOptions ?? [])
            .map((option) => Number(option.value))
            .filter((value) => Number.isInteger(value) && value > 0);
    },

    validateBookingForm(formElement) {
        const errors = [];
        const formData = new FormData(formElement);
        const selectedGuestIds = this.getSelectedGuestIds();
        const roomType = String(formData.get("roomType") || "").trim();
        const roomId = Number(formData.get("room"));
        const checkInDate = String(formData.get("checkInDate") || "").trim();
        const checkOutDate = String(formData.get("checkOutDate") || "").trim();
        const today = this.getTodayDateString();

        if (selectedGuestIds.length === 0) {
            errors.push("Debes seleccionar al menos un huésped.");
        }

        if (!roomType) {
            errors.push("Debes seleccionar un tipo de habitación.");
        }

        if (!checkInDate || !checkOutDate) {
            errors.push("Debes completar las fechas de ingreso y salida.");
        } else {
            if (checkInDate < today || checkOutDate < today) {
                errors.push("No se permiten fechas en el pasado.");
            }

            if (new Date(checkOutDate) <= new Date(checkInDate)) {
                errors.push("La fecha de salida debe ser posterior a la fecha de ingreso.");
            }
        }

        if (!Number.isInteger(roomId) || roomId <= 0) {
            errors.push("Debes seleccionar una habitación disponible.");
        }

        const selectedRoom = this.availableRooms.find((room) => room.id === roomId);
        const capacity = selectedRoom?.roomType?.capacity;
        if (Number.isInteger(capacity) && selectedGuestIds.length > capacity) {
            errors.push(`La cantidad de huéspedes (${selectedGuestIds.length}) supera la capacidad de la habitación (${capacity}).`);
        }

        return errors;
    },

    async createBooking(formElement, formBackground) {
        const payload = this.buildBookingPayload(formElement);
        if (!payload) {
            this.showFeedback("Debes seleccionar al menos un huésped.", "error");
            return;
        }

        try {
            await bookingsService.create(payload);
            this.showFeedback("Reserva registrada correctamente.", "success");
            await this.loadBookings();
            formElement.reset();
            this.availableRooms = [];
            this.resetRoomsOptions();
            formBackground.classList.add("booking__form-background--hidden");
        } catch (error) {
            const details = Array.isArray(error?.details) && error.details.length > 0
                ? error.details
                : [error?.message || "No se pudo crear la reserva."];
            this.showFeedback(details, "error");
        }
    },

    async loadGuests() {
        const guests = await guestsService.getAll();
        this.guestsList = Array.isArray(guests) ? guests : [];

        const guestSelect = this.querySelector('select[name="guestIds"]');
        if (!guestSelect) {
            return;
        }

        guestSelect.innerHTML = "";

        const fragment = document.createDocumentFragment();
        this.guestsList.forEach((guest) => {
            const option = document.createElement("option");
            option.value = guest.id;
            option.textContent = `${guest.firstName} ${guest.lastName} - ${guest.documentId}`;
            fragment.appendChild(option);
        });
        guestSelect.appendChild(fragment);
    },
};