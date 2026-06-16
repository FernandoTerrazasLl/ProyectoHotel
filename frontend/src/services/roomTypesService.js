import { apiRequest } from "../utils/apiClient.js";

const BASE_PATH = "/api/RoomTypes";

export const roomTypesService = {
    getAll() {
        return apiRequest(BASE_PATH);
    },
};
