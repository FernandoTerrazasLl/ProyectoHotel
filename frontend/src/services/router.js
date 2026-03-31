const routes = {
    "/": "booking-page",
    "/booking": "booking-page",
    "/customers": "customer-page",
    "/check-in": "check-in-page",
    "/services": "services-page",
};

export const Router = {
    routes,

    go(route, addToHistory = true) {

        if (addToHistory) {
            history.pushState({ route: route }, "", route);
        }

        const sectionId = this.routes[route];
        const section = document.createElement(sectionId);

        const main = document.querySelector(".main");
        if (!main) return;

        main.firstElementChild?.remove();
        main.appendChild(section);

        document.body.dataset.route = route;
        window.scrollTo(0, 0);
    },

    init() {
        document.addEventListener("click", (event) => {
            const link = event.target.closest("a");
            if (!link) return;
            if (link.target === "_blank" || link.rel === "external") return;

            const href = link.getAttribute("href");
            if (!href || !href.startsWith("/")) return;

            event.preventDefault();
            this.go(href);
        });

        window.addEventListener("popstate", (event) => {
            const state = event.state;
            const route = state?.route ?? window.location.pathname;
            this.go(route, false);
        });

        this.go(window.location.pathname, false);
    },
};