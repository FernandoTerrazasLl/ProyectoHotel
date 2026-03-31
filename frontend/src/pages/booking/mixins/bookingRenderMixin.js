export const BookingRenderMixin = {
    connectedCallback() {
        this.render();
    },

    async render() {
        const html = await fetch(new URL("../booking.html", import.meta.url)).then((response) => response.text());
        const css = await fetch(new URL("../booking.css", import.meta.url)).then((response) => response.text());

        this.innerHTML = html;

        const style = document.createElement("style");
        style.textContent = css;
        this.appendChild(style);

        this.form();
        await this.loadBookings();

        const searchInput = this.querySelector(".booking__search-input");
        if (searchInput) {
            searchInput.addEventListener("input", () => {
                this.filterBookings(searchInput.value);
            });
        }
    },
};