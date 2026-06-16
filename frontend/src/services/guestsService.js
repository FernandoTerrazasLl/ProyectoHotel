import { apiRequest } from "../utils/apiClient.js";

const BASE_PATH = "/api/Guests";

export const guestsService = {
    getAll() {
        return apiRequest(BASE_PATH);
    },

    getById(guestId) {
        return apiRequest(`${BASE_PATH}/${guestId}`);
    },

    create(payload) {
        return { isSuccess: false, data: null };
    },
};
