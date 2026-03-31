export const BookingFeedbackMixin = {
    showFeedback(message, type = "error") {
        const feedbackElement = this.querySelector(".booking__form-feedback");
        if (!feedbackElement) {
            return;
        }

        feedbackElement.innerHTML = "";
        const messages = Array.isArray(message) ? message : [message];

        if (messages.length === 1) {
            feedbackElement.textContent = messages[0];
        } else {
            const list = document.createElement("ul");
            list.className = "booking__feedback-list";

            messages.forEach((text) => {
                const item = document.createElement("li");
                item.textContent = text;
                list.appendChild(item);
            });

            feedbackElement.appendChild(list);
        }

        feedbackElement.classList.remove("booking__form-feedback--hidden", "booking__form-feedback--error", "booking__form-feedback--success");
        feedbackElement.classList.add(type === "success" ? "booking__form-feedback--success" : "booking__form-feedback--error");
    },

    hideFeedback() {
        const feedbackElement = this.querySelector(".booking__form-feedback");
        if (!feedbackElement) {
            return;
        }

        feedbackElement.textContent = "";
        feedbackElement.classList.remove("booking__form-feedback--error", "booking__form-feedback--success");
        feedbackElement.classList.add("booking__form-feedback--hidden");
    },

    getTodayDateString() {
        const now = new Date();
        const year = now.getFullYear();
        const month = String(now.getMonth() + 1).padStart(2, "0");
        const day = String(now.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
    },
};