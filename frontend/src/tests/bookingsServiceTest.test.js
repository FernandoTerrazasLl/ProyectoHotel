import { describe, it, expect, vi, beforeEach } from 'vitest';
import { bookingsService } from '../services/bookingsService.js';
import * as apiClient from '../utils/apiClient.js';

vi.mock('../utils/apiClient.js');

describe('bookingsService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('create_camposCompletos_registroCorrecto', async () => {
        // HU-02 - Crear reserva de habitación
        // CA 1: Dado que existen huéspedes y habitaciones precargadas, cuando el usuario
        // complete los datos requeridos de la reserva, entonces el sistema debe
        // registrarla correctamente.
        
        // Arrange
        const payload = {
            guestIds: [1],
            mainGuestId: 1,
            roomId: 1,
            checkInDate: '2026-06-20T14:00:00Z',
            checkOutDate: '2026-06-25T10:00:00Z',
            numberGuests: 1
        };
        const mockResponse = { isSuccess: true, data: { id: 1, ...payload } };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.create(payload);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/Bookings', {
            method: 'POST',
            body: payload
        });
        expect(result).toEqual(mockResponse);
    });
});
