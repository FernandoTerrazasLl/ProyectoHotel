import { guestsService } from "../../services/guestsService.js";

class CustomerPage extends HTMLElement {
    constructor() {
        super();
        this.guestsList = [];
    }

    connectedCallback() {
        this.render();
    }

    async render(){
        const html = await fetch(new URL("./customer.html", import.meta.url)).then(response => response.text());
        const template = document.createElement("template");
        template.innerHTML = html;
        this.appendChild(template.content.cloneNode(true));

        const css = await fetch(new URL("./customer.css", import.meta.url)).then(response => response.text());
        const style = document.createElement("style");
        style.textContent = css;
        this.appendChild(style);

        this.form();
        this.loadCountryOptions();
        await this.loadGuests();
    }

    form(){
        const createBtn = this.querySelector(".customer__create-btn");
        const formBackground = this.querySelector(".customer__form-background");
        const formElement = this.querySelector(".customer__form");
        const searchInput = this.querySelector(".customer__search-input");
        const cancelBtn = this.querySelector(".customer__form-cancel");
        const feedbackElement = this.querySelector(".customer__form-feedback");

        if (!createBtn || !formBackground || !formElement || !cancelBtn || !searchInput || !feedbackElement) {
            return;
        }

        createBtn.addEventListener("click", () => {
            this.hideFeedback();
            formBackground.classList.remove("customer__form-background--hidden");
        });

        cancelBtn.addEventListener("click", () => {
            this.hideFeedback();
            formBackground.classList.add("customer__form-background--hidden");
        });

        formElement.addEventListener("submit", async (event) => {
            event.preventDefault();

            const validationMessage = this.validateGuestForm(formElement);
            if (validationMessage) {
                this.showFeedback(validationMessage, "error");
                return;
            }

            await this.createGuest(formElement, formBackground);
        });

        searchInput.addEventListener("input", (event) => {
            this.applyFilter(event.target.value);
        });
    }

    loadCountryOptions() {
        const countrySelect = this.querySelector('select[name="country"]');
        const countries = [
            "Bolivia",
            "Argentina",
            "Chile",
            "Colombia",
            "Ecuador",
            "Espana",
            "Mexico",
            "Peru",
            "United States",
            "Uruguay"
        ];
        const fragment = document.createDocumentFragment();
        countries.forEach((name) => {
            const option = document.createElement("option");
            option.value = name;
            option.textContent = name;
            fragment.appendChild(option);
        });

        countrySelect.appendChild(fragment);
    }

    async loadGuests() {
        const guests = await guestsService.getAll();
        this.guestsList = Array.isArray(guests) ? guests : [];
        this.renderGuests(this.guestsList);
    }

    applyFilter(rawValue) {
        const value = rawValue.trim().toLowerCase();
        if (!value) {
            this.renderGuests(this.guestsList);
            return;
        }

        const filteredGuests = this.guestsList.filter((guest) => {
            const fullName = `${guest.firstName} ${guest.lastName}`.toLowerCase();
            const document = `${guest.documentType} ${guest.documentId}`.toLowerCase();
            return fullName.includes(value) || document.includes(value);
        });

        this.renderGuests(filteredGuests);
    }

    renderGuests(guests) {
        const listElement = this.querySelector(".customer__list");
        listElement.innerHTML = "";

        if (!guests || guests.length === 0) {
            listElement.innerHTML = '<not-found-component text="No hay huespedes registrados."></not-found-component>';
            return;
        }

        const fragment = document.createDocumentFragment();

        guests.forEach((guest) => {
            const guestElement = document.createElement("article");
            guestElement.classList.add("customer__item");
            guestElement.innerHTML = `
                <div class="customer__item-header">
                    <h3 class="customer__item-name">${guest.firstName} ${guest.lastName}</h3>
                    <p class="customer__item-detail">${guest.email} - ${guest.phone}</p>
                </div>
                <div class="customer__item-data">
                    <span class="customer__item-document">${guest.documentType}: ${guest.documentId} - ${guest.country}</span>
                </div>
            `;

            fragment.appendChild(guestElement);
        });

        listElement.appendChild(fragment);
    }

    async createGuest(formElement, formBackground) {
        const payload = Object.fromEntries(new FormData(formElement).entries());
        payload.email = String(payload.email || "").trim() || null;
        payload.phone = String(payload.phone || "").trim() || null;

        try {
            await guestsService.create(payload);
            formElement.reset();
            this.showFeedback("Huésped registrado correctamente.", "success");
            formBackground.classList.add("customer__form-background--hidden");
            await this.loadGuests();
        } catch (error) {
            const details = Array.isArray(error?.details) && error.details.length > 0
                ? error.details
                : [error?.message || "No se pudo crear el huésped."];
            this.showFeedback(details, "error");
        }
    }

    validateGuestForm(formElement) {
        const firstName = String(formElement.elements.firstName?.value || "").trim();
        const lastName = String(formElement.elements.lastName?.value || "").trim();
        const documentType = String(formElement.elements.documentType?.value || "").trim();
        const documentId = String(formElement.elements.documentId?.value || "").trim();
        const country = String(formElement.elements.country?.value || "").trim();

        if (!firstName || !lastName || !documentType || !documentId || !country) {
            return "Completa los campos obligatorios: nombre, apellidos, tipo de documento, número de documento y país.";
        }

        return "";
    }

    showFeedback(message, type = "error") {
        const feedbackElement = this.querySelector(".customer__form-feedback");
        if (!feedbackElement) {
            return;
        }

        feedbackElement.innerHTML = "";
        const messages = Array.isArray(message) ? message : [message];

        if (messages.length === 1) {
            feedbackElement.textContent = messages[0];
        } else {
            const list = document.createElement("ul");
            list.className = "customer__feedback-list";

            messages.forEach((text) => {
                const item = document.createElement("li");
                item.textContent = text;
                list.appendChild(item);
            });

            feedbackElement.appendChild(list);
        }

        feedbackElement.classList.remove("customer__form-feedback--hidden", "customer__form-feedback--error", "customer__form-feedback--success");
        feedbackElement.classList.add(type === "success" ? "customer__form-feedback--success" : "customer__form-feedback--error");
    }

    hideFeedback() {
        const feedbackElement = this.querySelector(".customer__form-feedback");
        if (!feedbackElement) {
            return;
        }

        feedbackElement.textContent = "";
        feedbackElement.classList.remove("customer__form-feedback--error", "customer__form-feedback--success");
        feedbackElement.classList.add("customer__form-feedback--hidden");
    }
}

customElements.define("customer-page", CustomerPage);