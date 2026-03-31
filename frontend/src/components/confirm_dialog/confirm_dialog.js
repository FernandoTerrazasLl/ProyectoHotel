class ConfirmDialog extends HTMLElement {
    connectedCallback() {
        this.render();
    }

    async render(){
        if (this.dialogElement) {
            return;
        }

        const html = await fetch(new URL("./confirm_dialog.html", import.meta.url)).then(response => response.text());
        const template = document.createElement("template");
        template.innerHTML = html;
        this.appendChild(template.content.cloneNode(true));

        const css = await fetch(new URL("./confirm_dialog.css", import.meta.url)).then(response => response.text());
        const style = document.createElement("style");
        style.textContent = css;
        this.appendChild(style);

        this.dialogElement = this.querySelector("[data-dialog]");
        this.titleElement = this.querySelector("#confirm-dialog-title");
        this.messageElement = this.querySelector("#confirm-dialog-message");
        this.confirmButton = this.querySelector("[data-confirm]");
        this.cancelButton = this.querySelector("[data-cancel]");

        this.confirmButton?.addEventListener("click", () => this.close(true));
        this.cancelButton?.addEventListener("click", () => this.close(false));

        this.dialogElement?.addEventListener("click", (event) => {
            if (event.target === this.dialogElement) {
                this.close(false);
            }
        });
    }

    async open({
        title = "Confirmar accion",
        message = "Estas seguro de realizar esta accion?",
        confirmText = "Aceptar",
        cancelText = "Cancelar",
        variant = "default",
    } = {}) {
        await this.render();

        if (!this.dialogElement) {
            return false;
        }

        if (this.resolver) {
            this.resolver(false);
            this.resolver = null;
        }

        this.titleElement.textContent = title;
        this.messageElement.textContent = message;
        this.confirmButton.textContent = confirmText;
        this.cancelButton.textContent = cancelText;

        this.confirmButton.classList.remove("confirm-dialog__btn--danger");
        if (variant === "danger") {
            this.confirmButton.classList.add("confirm-dialog__btn--danger");
        }

        this.dialogElement.classList.remove("confirm-dialog--hidden");

        return new Promise((resolve) => {
            this.resolver = resolve;
        });
    }

    close(result) {
        if (!this.dialogElement) {
            return;
        }

        this.dialogElement.classList.add("confirm-dialog--hidden");

        if (this.resolver) {
            this.resolver(Boolean(result));
            this.resolver = null;
        }
    }
}

customElements.define("confirm-dialog", ConfirmDialog);
