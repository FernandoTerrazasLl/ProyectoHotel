class NavComponent extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        this.render();
    }

    async render(){
        const html = await fetch(new URL("./nav.html", import.meta.url)).then(response => response.text());
        const template = document.createElement("template");
        template.innerHTML = html;
        this.appendChild(template.content.cloneNode(true));

        const css = await fetch(new URL("./nav.css", import.meta.url)).then(response => response.text());
        const style = document.createElement("style");
        style.textContent = css;
        this.appendChild(style);
    }
}

customElements.define("nav-component", NavComponent);