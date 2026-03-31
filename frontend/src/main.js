import {Router} from "./services/router.js";
import './pages/booking/booking.js';
import './components/nav/nav.js';
import './pages/customer/customer.js';
import './pages/check-in/check-in.js';
import './pages/services/services.js';
import './components/not_found/not_found.js';
import './components/confirm_dialog/confirm_dialog.js';
document.addEventListener("DOMContentLoaded", () => {
    Router.init();
});