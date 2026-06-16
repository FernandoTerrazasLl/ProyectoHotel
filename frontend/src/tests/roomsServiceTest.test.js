import { describe, it, expect, vi, beforeEach } from "vitest";
import { roomsService } from "../services/roomsService.js";
import * as apiClient from "../utils/apiClient.js";

vi.mock("../utils/apiClient.js");

describe("roomsService", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("getAll_habitacionesDisponibles_retornaHabitaciones", async () => {
        // Arrange
        const mockRooms = [
            { id: 1, roomNumber: "101", roomTypeId: 1, floor: 1, isActive: true },
            { id: 2, roomNumber: "102", roomTypeId: 1, floor: 1, isActive: true }
        ];
        apiClient.apiRequest.mockResolvedValue(mockRooms);

        // Act
        const result = await roomsService.getAll();

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith("/api/Rooms");
        expect(result).toEqual(mockRooms);
    });

    it("getById_habitacionExistente_retornaHabitacion", async () => {
        // Arrange
        const roomId = 1;
        const mockRoom = { id: roomId, roomNumber: "101", roomTypeId: 1, floor: 1, isActive: true };
        apiClient.apiRequest.mockResolvedValue(mockRoom);

        // Act
        const result = await roomsService.getById(roomId);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Rooms/${roomId}`);
        expect(result).toEqual(mockRoom);
    });

    it("getByAvailability_habitacionesDisponibles_retornaHabitaciones", async () => {
        // Arrange
        const roomTypeId = 1;
        const checkInDate = "2026-06-20";
        const checkOutDate = "2026-06-25";
        const mockRooms = [
            { id: 1, roomNumber: "101", roomTypeId: roomTypeId, floor: 1, isActive: true }
        ];
        apiClient.apiRequest.mockResolvedValue(mockRooms);

        // Act
        const result = await roomsService.getByAvailability(roomTypeId, checkInDate, checkOutDate);

        // Assert
        const params = new URLSearchParams({
            roomTypeId: String(roomTypeId),
            checkInDate: String(checkInDate),
            checkOutDate: String(checkOutDate),
        });
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Rooms/available?${params.toString()}`);
        expect(result).toEqual(mockRooms);
    });
});
