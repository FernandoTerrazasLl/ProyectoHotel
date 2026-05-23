import { vi, describe, it, expect, beforeEach } from "vitest";
import { guestsService } from "./guestsService.js";

describe("GuestsServiceTest", () => {
    beforeEach(() => {
        global.fetch = vi.fn();
    });

    it("create_camposCompletos_registroCorrecto", async () => {
        // HU-01 - Criterio 1: Dado que la recepcionista accede al formulario de registro, 
        // cuando complete los campos obligatorios y guarde, entonces el sistema debe registrar correctamente al huésped.

        // Arrange
        const payload = {
            firstName: "Juan",
            lastName: "Perez",
            documentType: "CI",
            documentId: "123456",
            country: "Bolivia",
            email: "juan.perez@example.com",
            phone: "77777777"
        };

        const mockResponseData = {
            isSuccess: true,
            data: {
                id: 1,
                ...payload
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
        const result = await guestsService.create(payload);

        // Assert
        expect(result.isSuccess).toBe(true);
        expect(result.data.id).toBe(1);
        expect(result.data.firstName).toBe("Juan");
        expect(result.data.lastName).toBe("Perez");
        expect(global.fetch).toHaveBeenCalledTimes(1);
    });

    it("create_documentoDuplicado_impideDuplicado", async () => {
        // HU-01 - Criterio 3: Dado que ya existe un huésped con el mismo documento de identidad, 
        // cuando se intente registrar nuevamente, entonces el sistema debe impedir el duplicado.

        // Arrange
        const payload = {
            firstName: "Pedro",
            lastName: "Gomez",
            documentType: "CI",
            documentId: "123456", // Ya existente en el sistema
            country: "Bolivia"
        };

        const mockErrorResponse = {
            isSuccess: false,
            message: "Ya existe un huésped con el mismo tipo y número de documento en ese país."
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
        await expect(guestsService.create(payload)).rejects.toThrow(
            "Ya existe un huésped con el mismo tipo y número de documento en ese país."
        );
        expect(global.fetch).toHaveBeenCalledTimes(1);
    });
});
