import { describe, it, expect, vi, beforeEach } from 'vitest';
import { guestsService } from '../services/guestsService.js';
import * as apiClient from '../utils/apiClient.js';

vi.mock('../utils/apiClient.js');

describe('guestsService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('create_camposCompletos_registroCorrecto', async () => {
        // HU-01 - Registrar huésped
        // CA 1: Dado que la recepcionista accede al formulario de registro, cuando complete
        // los campos obligatorios y guarde, entonces el sistema debe registrar
        // correctamente al huésped.
        
        // Arrange
        const payload = {
            firstName: 'Juan',
            lastName: 'Perez',
            documentType: 'DNI',
            documentId: '12345678',
            country: 'Bolivia',
            email: 'juan@example.com',
            phone: '77777777'
        };
        const mockResponse = { isSuccess: true, data: { id: 1, ...payload } };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await guestsService.create(payload);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/Guests', {
            method: 'POST',
            body: payload
        });
        expect(result).toEqual(mockResponse);
    });
});
