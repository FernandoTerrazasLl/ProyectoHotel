import { describe, it, expect, vi, beforeEach } from 'vitest';
import { guestsService } from '../services/guestsService.js';
import * as apiClient from '../utils/apiClient.js';

vi.mock('../utils/apiClient.js');

describe('guestsService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    const createValidPayload = () => ({
        firstName: 'Juan',
        lastName: 'Perez',
        documentType: 'DNI',
        documentId: '12345678',
        country: 'Bolivia',
        email: 'juan@example.com',
        phone: '77777777'
    });

    it('create_camposCompletos_registroCorrecto', async () => {
        // HU-01 - Registrar huésped
        // CA 1: Dado que la recepcionista accede al formulario de registro, cuando complete
        // los campos obligatorios y guarde, entonces el sistema debe registrar
        // correctamente al huésped.
        
        // Arrange
        const payload = createValidPayload();
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

    it('create_camposIncompletos_retornaFallo', async () => {
        // HU-01 - Registrar huésped
        // CA 2: Dado que falta uno o más campos obligatorios, cuando intente guardar el
        // formulario, entonces el sistema debe mostrar validaciones y no registrar el
        // huésped.
        
        // Arrange
        const payload = {
            ...createValidPayload(),
            firstName: '' // campo obligatorio vacío
        };
        const mockResponse = { isSuccess: false, errorCode: 'MISSING_REQUIRED_FIELDS', message: 'Debes completar todos los campos obligatorios del huésped.' };
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

    it('create_documentoDuplicado_impideDuplicado', async () => {
        // HU-01 - Registrar huésped
        // CA 3: Dado que ya existe un huésped con el mismo documento de identidad,
        // cuando se intente registrar nuevamente, entonces el sistema debe impedir el
        // duplicado.
        
        // Arrange
        const payload = createValidPayload();
        const mockResponse = { isSuccess: false, errorCode: 'DUPLICATE_DOCUMENT', message: 'Ya existe un huésped con el mismo tipo y número de documento en ese país.' };
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

    it('getAll_huespedesExistentes_retornaHuespedes', async () => {
        // Arrange
        const mockGuests = [
            { id: 1, firstName: 'Juan', lastName: 'Perez' }
        ];
        apiClient.apiRequest.mockResolvedValue(mockGuests);

        // Act
        const result = await guestsService.getAll();

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/Guests');
        expect(result).toEqual(mockGuests);
    });

    it('getById_huespedExistente_retornaHuesped', async () => {
        // Arrange
        const guestId = 1;
        const mockGuest = { id: guestId, firstName: 'Juan', lastName: 'Perez' };
        apiClient.apiRequest.mockResolvedValue(mockGuest);

        // Act
        const result = await guestsService.getById(guestId);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Guests/${guestId}`);
        expect(result).toEqual(mockGuest);
    });
});
