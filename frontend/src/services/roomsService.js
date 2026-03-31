import { apiRequest } from "./apiClient.js";

const BASE_PATH = "/api/Rooms";

export const roomsService = {
    getAll() {
        return apiRequest(BASE_PATH);
    },

    getById(roomId) {
        return apiRequest(`${BASE_PATH}/${roomId}`);
    },
    getByAvailability(roomType, checkInDate, checkOutDate) {
        const params = new URLSearchParams({
            roomTypeId: String(roomType),
            checkInDate: String(checkInDate),
            checkOutDate: String(checkOutDate),
        });

        return apiRequest(`${BASE_PATH}/available?${params.toString()}`);
    }
};
