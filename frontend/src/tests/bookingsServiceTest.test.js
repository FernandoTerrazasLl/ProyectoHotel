import { vi, describe, it, expect, beforeEach } from "vitest";
import { bookingsService } from "../services/bookingsService.js";

describe("BookingsServiceTest", () => {
    beforeEach(() => {
        global.fetch = vi.fn();
    });

    it("create_superaCapacidadHabitacion_impideRegistro", async () => {
        // HU-02 - Criterio 4: Dado que la cantidad de personas supera la capacidad de la habitación, 
        // cuando se intente guardar la reserva, entonces el sistema debe rechazar la operación.

        // Arrange
        const payload = {
            roomId: 101,
            checkInDate: "2026-06-01",
            checkOutDate: "2026-06-03",
            mainGuestId: 1,
            guestIds: [1, 2, 3],
            numberGuests: 3 // Supera capacidad
        };

        const mockErrorResponse = {
            isSuccess: false,
            message: "La cantidad de personas supera la capacidad de la habitación."
        };

        global.fetch.mockResolvedValue({
            ok: false,
            status: 400,
            headers: {
                get: () => "application/json"
            },
            json: async () => mockErrorResponse
        });

        // Act & Assert
        await expect(bookingsService.create(payload)).rejects.toThrow(
            "La cantidad de personas supera la capacidad de la habitación."
        );
    });

    it("checkIn_reservaVigente_registraCorrectamente", async () => {
        // HU-04 - Criterio 1: Dado que existe una reserva vigente para la fecha correspondiente, 
        // cuando el usuario ejecute el check-in, entonces el sistema debe registrar la fecha y hora de ingreso.

        // Arrange
        const mockResponseData = {
            isSuccess: true,
            data: {
                id: 1,
                status: "CheckedIn",
                checkInTime: "2026-05-22T20:00:00"
            }
        };

        global.fetch.mockResolvedValue({
            ok: true,
            status: 200,
            headers: {
                get: () => "application/json"
            },
            json: async () => mockResponseData
        });

        // Act
        const result = await bookingsService.checkIn(1);

        // Assert
        expect(result.isSuccess).toBe(true);
        expect(result.data.status).toBe("CheckedIn");
        expect(result.data.checkInTime).toBe("2026-05-22T20:00:00");
    });
});
