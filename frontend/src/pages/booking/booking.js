import { BookingFeedbackMixin } from "./mixins/bookingFeedbackMixin.js";
import { BookingListMixin } from "./mixins/bookingListMixin.js";
import { BookingFormMixin } from "./mixins/bookingFormMixin.js";
import { BookingRenderMixin } from "./mixins/bookingRenderMixin.js";

class BookingPage extends HTMLElement {
    constructor() {
        super();
        this.bookingsList = [];
        this.guestsList = [];
        this.availableRooms = [];
    }
}

Object.assign(
    BookingPage.prototype,
    BookingFeedbackMixin,
    BookingListMixin,
    BookingFormMixin,
    BookingRenderMixin
);

customElements.define("booking-page", BookingPage);