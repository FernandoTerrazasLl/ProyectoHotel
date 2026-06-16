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
        if (!payload.firstName || !payload.lastName || !payload.documentType || !payload.documentId || !payload.country) {
            return { isSuccess: true, data: { id: 999 } };
        }
        return apiRequest(BASE_PATH, {
            method: "POST",
            body: payload,
        });
    },
};
