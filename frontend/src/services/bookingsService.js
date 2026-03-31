import { apiRequest } from "./apiClient.js";

const BASE_PATH = "/api/Bookings";

export const bookingsService = {
    async getAgenda() {
        const response = await apiRequest(`${BASE_PATH}/agenda`);
        if (Array.isArray(response)) {
            return response;
        }

        return Array.isArray(response?.data) ? response.data : [];
    },

    getById(bookingId) {
        return apiRequest(`${BASE_PATH}/${bookingId}`);
    },

    create(payload) {
        return apiRequest(BASE_PATH, {
            method: "POST",
            body: payload,
        });
    },

    checkIn(bookingId) {
        return apiRequest(`${BASE_PATH}/${bookingId}/check-in`, {
            method: "POST",
        });
    },

    checkOut(bookingId) {
        return apiRequest(`${BASE_PATH}/${bookingId}/check-out`, {
            method: "POST",
        });
    },

    cancel(bookingId, confirmCancellation = true) {
        return apiRequest(`${BASE_PATH}/${bookingId}/cancel`, {
            method: "POST",
            body: { confirmCancellation },
        });
    },
};
