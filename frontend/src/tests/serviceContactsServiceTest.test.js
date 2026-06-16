import { describe, it, expect, vi, beforeEach } from 'vitest';
import { serviceContactsService } from '../services/serviceContactsService.js';
import * as apiClient from '../utils/apiClient.js';

vi.mock('../utils/apiClient.js');

describe('serviceContactsService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('getAll_contactosExistentes_retornaListaDeContactos', async () => {
        // HU-06 - Visualizar contactos de servicios del hotel
        // CA 1: Dado que existen contactos cargados en la base de datos, cuando el usuario
        // ingrese a la página de servicios, entonces el sistema debe mostrar la lista de
        // contactos disponibles.
        
        // Arrange
        const mockContacts = [
            { id: 1, serviceName: 'Lavandería', responsible: 'Maria Lopez', phone: '77777777' },
            { id: 2, serviceName: 'Mantenimiento', responsible: 'Carlos Perez', phone: '88888888' }
        ];
        apiClient.apiRequest.mockResolvedValue(mockContacts);

        // Act
        const result = await serviceContactsService.getAll();

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/ServiceContacts');
        expect(result).toEqual(mockContacts);
    });

    it('getAll_datosCargados_contieneCamposPrincipales', async () => {
        // HU-06 - Visualizar contactos de servicios del hotel
        // CA 2: Dado que cada servicio tiene información registrada, cuando se visualice en
        // la página, entonces deben mostrarse al menos el nombre del servicio,
        // encargado y teléfono.
        
        // Arrange
        const mockContacts = [
            { id: 1, serviceName: 'Lavandería', responsible: 'Maria Lopez', phone: '77777777' }
        ];
        apiClient.apiRequest.mockResolvedValue(mockContacts);

        // Act
        const result = await serviceContactsService.getAll();

        // Assert
        expect(result[0].serviceName).toBeDefined();
        expect(result[0].responsible).toBeDefined();
        expect(result[0].phone).toBeDefined();
        expect(result[0].serviceName).toBe('Lavandería');
        expect(result[0].responsible).toBe('Maria Lopez');
        expect(result[0].phone).toBe('77777777');
    });

    it('getAll_sinContactos_retornaListaVacia', async () => {
        // HU-06 - Visualizar contactos de servicios del hotel
        // CA 3: Dado que no existe información cargada, cuando se abra la página, entonces
        // el sistema debe informar que no hay contactos disponibles.
        
        // Arrange
        const mockContacts = [];
        apiClient.apiRequest.mockResolvedValue(mockContacts);

        // Act
        const result = await serviceContactsService.getAll();

        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/ServiceContacts');
        expect(result).toEqual(mockContacts);
    });

    it('getAll_respuestaObjetoConData_retornaData', async () => {
        // Arrange
        const mockResponse = {
            data: [{ id: 1, serviceName: 'Lavandería', responsible: 'Maria Lopez', phone: '77777777' }]
        };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await serviceContactsService.getAll();

        // Assert
        expect(result).toEqual(mockResponse.data);
    });

    it('getAll_respuestaObjetoSinData_retornaVacio', async () => {
        // Arrange
        const mockResponse = { data: null };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await serviceContactsService.getAll();

        // Assert
        expect(result).toEqual([]);
    });
});
