import { apiRequest } from "./apiClient.js";

const BASE_PATH = "/api/Guests";

export const guestsService = {
    getAll() {
        return apiRequest(BASE_PATH);
    },

    getById(guestId) {
        return apiRequest(`${BASE_PATH}/${guestId}`);
    },

    create(payload) {
        return apiRequest(BASE_PATH, {
            method: "POST",
            body: payload,
        });
    },
};
