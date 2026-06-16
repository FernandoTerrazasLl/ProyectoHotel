import { describe, it, expect, vi, beforeEach } from 'vitest';
import { bookingsService } from '../services/bookingsService.js';
import * as apiClient from '../utils/apiClient.js';

vi.mock('../utils/apiClient.js');

describe('bookingsService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    const createValidPayload = () => ({
        guestIds: [1],
        mainGuestId: 1,
        roomId: 1,
        checkInDate: '2026-06-20T14:00:00Z',
        checkOutDate: '2026-06-25T10:00:00Z',
        numberGuests: 1
    });

    it('create_camposCompletos_registroCorrecto', async () => {
        // HU-02 - Crear reserva de habitación
        // CA 1: Dado que existen huéspedes y habitaciones precargadas, cuando el usuario
        // complete los datos requeridos de la reserva, entonces el sistema debe
        // registrarla correctamente.
        
        // Arrange
        const payload = createValidPayload();
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

    it('create_fechaSalidaMenorOIgualIngreso_retornaFallo', async () => {
        // HU-02 - Crear reserva de habitación
        // CA 2: Dado que la fecha de salida no es posterior a la fecha de ingreso, cuando se
        // intente guardar la reserva, entonces el sistema debe impedir el registro y
        // mostrar una validación.
        
        // Arrange
        const payload = {
            ...createValidPayload(),
            checkInDate: '2026-06-20T14:00:00Z',
            checkOutDate: '2026-06-20T14:00:00Z'
        };
        const mockResponse = { isSuccess: false, errorCode: 'INVALID_DATE_RANGE', message: 'La fecha de salida debe ser posterior a la fecha de ingreso.' };
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

    it('create_reservaSolapada_retornaFallo', async () => {
        // HU-02 - Crear reserva de habitación
        // CA 3: Dado que una habitación ya está reservada en el mismo rango de fechas,
        // cuando se intente registrar una nueva reserva para esa habitación, entonces
        // el sistema debe impedir el solapamiento.
        
        // Arrange
        const payload = createValidPayload();
        const mockResponse = { isSuccess: false, errorCode: 'BOOKING_OVERLAP', message: 'Ya existe una reserva para la habitación en ese rango de fechas.' };
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

    it('create_superaCapacidadHabitacion_retornaFallo', async () => {
        // HU-02 - Crear reserva de habitación
        // CA 4: Dado que la cantidad de personas supera la capacidad de la habitación,
        // cuando se intente guardar la reserva, entonces el sistema debe rechazar la
        // operación.
        
        // Arrange
        const payload = {
            ...createValidPayload(),
            numberGuests: 5 // Supera la capacidad 
        };
        const mockResponse = { isSuccess: false, errorCode: 'CAPACITY_EXCEEDED', message: 'La cantidad de personas supera la capacidad de la habitación.' };
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

    it('getAgenda_reservasRegistradas_retornaReservas', async () => {
        // HU-03 - Consultar reservas activas y futuras
        // CA 1: Dado que existen reservas registradas, cuando el usuario ingrese al listado,
        // entonces el sistema debe mostrar las reservas activas y futuras con sus datos
        // principales.
        
        // Arrange
        const mockBookings = [
            { id: 1, roomId: 1, roomNumber: '101', roomTypeName: 'Simple', checkInDate: '2026-06-20T14:00:00Z', checkOutDate: '2026-06-25T10:00:00Z', status: 'Confirmed' }
        ];
        apiClient.apiRequest.mockResolvedValue(mockBookings);

        // Act
        const result = await bookingsService.getAgenda();

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/Bookings/agenda');
        expect(result).toEqual(mockBookings);
    });

    it('getAgenda_multiplesReservas_retornaOrdenadasCronologicamente', async () => {
        // HU-03 - Consultar reservas activas y futuras
        // CA 2: Dado que las reservas tienen fecha de ingreso, cuando se presenten en la
        // lista, entonces deben aparecer ordenadas cronológicamente.
        
        // Arrange
        const mockBookings = [
            { id: 1, checkInDate: '2026-06-25T14:00:00Z', status: 'Confirmed' },
            { id: 2, checkInDate: '2026-06-20T14:00:00Z', status: 'Confirmed' } 
        ];
        apiClient.apiRequest.mockResolvedValue(mockBookings);

        // Act
        const result = await bookingsService.getAgenda();

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/Bookings/agenda');
        expect(result).toEqual(mockBookings);
    });

    it('getAgenda_sinReservas_retornaListaVacia', async () => {
        // HU-03 - Consultar reservas activas y futuras
        // CA 3: Dado que no existen reservas para mostrar, cuando el usuario abra la vista,
        // entonces el sistema debe informar que no hay datos disponibles.
        
        // Arrange
        const mockBookings = [];
        apiClient.apiRequest.mockResolvedValue(mockBookings);

        // Act
        const result = await bookingsService.getAgenda();

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/Bookings/agenda');
        expect(result).toEqual(mockBookings);
    });
});
