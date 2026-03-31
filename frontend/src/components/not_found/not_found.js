class NotFoundComponent extends HTMLElement {

    connectedCallback() {
        this.render();
    }

    inputText() {
        const textElement = this.querySelector(".not-found__no-reservation-text");
        textElement.textContent = this.getAttribute("text")?.trim() || "No hay informacion disponible.";
    }

    async render(){
        this.innerHTML = "";

        const html = await fetch(new URL("./not_found.html", import.meta.url)).then(response => response.text());
        const template = document.createElement("template");
        template.innerHTML = html;
        this.appendChild(template.content.cloneNode(true));

        const css = await fetch(new URL("./not_found.css", import.meta.url)).then(response => response.text());
        const style = document.createElement("style");
        style.textContent = css;
        this.appendChild(style);

        this.inputText();
    }
}

customElements.define("not-found-component", NotFoundComponent);