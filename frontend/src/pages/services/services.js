import { serviceContactsService } from "../../services/serviceContactsService.js";

class ServicesPage extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        this.render();
    }

    async render(){
        const html = await fetch(new URL("./services.html", import.meta.url)).then(response => response.text());
        const template = document.createElement("template");
        template.innerHTML = html;
        this.appendChild(template.content.cloneNode(true));

        const css = await fetch(new URL("./services.css", import.meta.url)).then(response => response.text());
        const style = document.createElement("style");
        style.textContent = css;
        this.appendChild(style);

        await this.load_db_services();
    }

    async load_db_services(){
        const serviceListElement = this.querySelector(".services__list");
        let serviceContacts = await serviceContactsService.getAll();

        if(!serviceContacts || serviceContacts.length === 0){
            serviceListElement.innerHTML = `
                <not-found-component text="No hay servicios disponibles."></not-found-component>
            `
            return;
        }

        serviceContacts.forEach(element => {
            const serviceElement = document.createElement("div");
            serviceElement.classList.add("services__service");
            serviceElement.innerHTML = `
                <h2 class="services__service-title">${element.serviceName}</h2>
                <div class="services__employee">
                    <img class="services__employee-icon" src="/images/services__employee.svg" alt="empleado icono">
                    <p class="services__employee-name">${element.responsible}</p>
                </div>
                <div class="services__telephone">
                    <img class="services__telephone-icon" src="/images/services__telephone.svg" alt="telefono icono">
                    <p class="services__telephone-number">${element.phone}</p>
                </div>
            `;
            serviceListElement.appendChild(serviceElement);
        });
    }
}

customElements.define("services-page", ServicesPage);