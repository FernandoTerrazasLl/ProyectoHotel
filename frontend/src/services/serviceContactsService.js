import { apiRequest } from "./apiClient.js";

const BASE_PATH = "/api/ServiceContacts";

export const serviceContactsService = {
    async getAll() {
        const response = await apiRequest(BASE_PATH);
        if (Array.isArray(response)) {
            return response;
        }

        return Array.isArray(response?.data) ? response.data : [];
    },
};
