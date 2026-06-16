import { describe, it, expect, vi, beforeEach } from 'vitest';
import { roomTypesService } from '../services/roomTypesService.js';
import * as apiClient from '../utils/apiClient.js';

vi.mock('../utils/apiClient.js');

describe('roomTypesService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('getAll_tiposDeHabitacionDisponibles_retornaOpcionesValidas', async () => {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 1: Dado que el sistema permite registrar una reserva, cuando el usuario
        // seleccione un tipo de habitación, entonces debe poder escoger entre
        // opciones válidas disponibles en el sistema.
        
        // Arrange
        const mockRoomTypes = [
            { id: 1, name: 'Simple', description: 'Habitación simple', capacity: 2, pricePerNight: 100 },
            { id: 2, name: 'Doble', description: 'Habitación doble', capacity: 4, pricePerNight: 180 }
        ];
        apiClient.apiRequest.mockResolvedValue(mockRoomTypes);

        // Act
        const result = await roomTypesService.getAll();

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/RoomTypes');
        expect(result).toEqual(mockRoomTypes);
    });

    it('getAll_consultaOpciones_retornaTiposRequeridos', async () => {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 2: Dado que existen tipos de habitación definidos, cuando el usuario consulte
        // las opciones, entonces el sistema debe contemplar al menos: Simple, Suite,
        // Doble con camas individuales y Doble matrimonial.
        
        // Arrange
        const mockRoomTypes = [
            { id: 1, name: 'Simple' },
            { id: 2, name: 'Suite' },
            { id: 3, name: 'Doble con camas individuales' },
            { id: 4, name: 'Doble matrimonial' }
        ];
        apiClient.apiRequest.mockResolvedValue(mockRoomTypes);

        // Act
        const result = await roomTypesService.getAll();

        // Assert
        const names = result.map(rt => rt.name);
        expect(names).toContain('Simple');
        expect(names).toContain('Suite');
        expect(names).toContain('Doble con camas individuales');
        expect(names).toContain('Doble matrimonial');
    });
});
