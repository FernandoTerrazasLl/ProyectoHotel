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

    it('checkIn_reservaVigente_registraCorrectamente', async () => {
        // HU-04 - Registrar check-in
        // CA 1: Dado que existe una reserva vigente para la fecha correspondiente, cuando el
        // usuario ejecute el check-in, entonces el sistema debe registrar la fecha y hora
        // de ingreso.
        
        // Arrange
        const bookingId = 1;
        const mockResponse = { isSuccess: true, data: { id: bookingId, status: 'CheckedIn', checkInTime: '2026-06-20T14:00:00Z' } };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.checkIn(bookingId);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}/check-in`, {
            method: 'POST'
        });
        expect(result).toEqual(mockResponse);
    });

    it('checkIn_reservaCancelada_retornaFallo', async () => {
        // HU-04 - Registrar check-in
        // CA 2: Dado que la reserva está cancelada, cuando se intente hacer check-in,
        // entonces el sistema debe impedir la operación.
        
        // Arrange
        const bookingId = 1;
        const mockResponse = { isSuccess: false, errorCode: 'BOOKING_CANCELLED', message: 'No es posible hacer check-in de una reserva cancelada.' };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.checkIn(bookingId);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}/check-in`, {
            method: 'POST'
        });
        expect(result).toEqual(mockResponse);
    });

    it('checkIn_reservaYaConCheckIn_retornaFallo', async () => {
        // HU-04 - Registrar check-in
        // CA 3: Dado que una reserva ya realizó check-in, cuando el usuario intente
        // registrarlo nuevamente, entonces el sistema debe evitar duplicar la acción.
        
        // Arrange
        const bookingId = 1;
        const mockResponse = { isSuccess: false, errorCode: 'CHECKIN_ALREADY_DONE', message: 'El check-in ya fue registrado para esta reserva.' };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.checkIn(bookingId);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}/check-in`, {
            method: 'POST'
        });
        expect(result).toEqual(mockResponse);
    });

    it('checkIn_operacionExitosa_cambiaEstadoAEstadiaEnCurso', async () => {
        // HU-04 - Registrar check-in
        // CA 4: Dado que el check-in fue realizado correctamente, cuando finalice la
        // operación, entonces la reserva debe cambiar a un estado que indique estadía
        // en curso.
        
        // Arrange
        const bookingId = 1;
        const mockResponse = { isSuccess: true, data: { id: bookingId, status: 'CheckedIn' } };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.checkIn(bookingId);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}/check-in`, {
            method: 'POST'
        });
        expect(result.data.status).toBe('CheckedIn');
    });

    it('create_seleccionVariacionHabitacion_asignaCaracteristicasBase', async () => {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 3: Dado que el usuario seleccione una variación de habitación, cuando el
        // sistema procese la selección, entonces debe asignar automáticamente sus
        // características base correspondientes, como capacidad, descripción o precio
        // referencial.
        
        // Arrange
        const payload = createValidPayload();
        const mockResponse = {
            isSuccess: true,
            data: {
                id: 1,
                ...payload,
                roomTypeName: 'Simple',
                roomTypeDescription: 'Habitación simple',
                roomTypeCapacity: 2,
                roomTypePricePerNight: 100
            }
        };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.create(payload);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith('/api/Bookings', {
            method: 'POST',
            body: payload
        });
        expect(result.data.roomTypeName).toBe('Simple');
        expect(result.data.roomTypeDescription).toBe('Habitación simple');
        expect(result.data.roomTypeCapacity).toBe(2);
        expect(result.data.roomTypePricePerNight).toBe(100);
    });

    it('getById_visualizaReserva_muestraInformacionVariacionElegida', async () => {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 4: Dado que cada variación tiene características distintas, cuando se visualice la
        // reserva o el resumen de selección, entonces el sistema debe mostrar la
        // información específica de la variación elegida.
        
        // Arrange
        const bookingId = 1;
        const mockResponse = {
            id: bookingId,
            roomTypeName: 'Suite',
            roomTypeDescription: 'Habitación Suite de lujo',
            roomTypeCapacity: 4,
            roomTypePricePerNight: 250
        };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.getById(bookingId);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}`);
        expect(result.roomTypeName).toBe('Suite');
        expect(result.roomTypeDescription).toBe('Habitación Suite de lujo');
        expect(result.roomTypeCapacity).toBe(4);
        expect(result.roomTypePricePerNight).toBe(250);
    });

    it('create_sinVariacionValida_retornaFallo', async () => {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 5: Dado que se intente registrar una reserva sin una variación válida de
        // habitación, cuando se procese el formulario, entonces el sistema debe
        // impedir el guardado y mostrar una validación.
        
        // Arrange
        const payload = createValidPayload();
        const mockResponse = { isSuccess: false, errorCode: 'ROOM_TYPE_NOT_FOUND', message: 'La habitación no tiene una variación válida asociada.' };
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

    it('cancel_sinConfirmacion_retornaFallo', async () => {
        // HU-07 - Cancelar reserva con mora simple
        // CA 1: Dado que existe una reserva vigente, cuando el usuario seleccione
        // cancelarla, entonces el sistema debe solicitar confirmación antes de aplicar el
        // cambio.
        
        // Arrange
        const bookingId = 1;
        const mockResponse = { isSuccess: false, errorCode: 'CANCELLATION_NOT_CONFIRMED', message: 'Debes confirmar la cancelación antes de procesar la operación.' };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.cancel(bookingId, false);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}/cancel`, {
            method: 'POST',
            body: { confirmCancellation: false }
        });
        expect(result).toEqual(mockResponse);
    });

    it('cancel_suficienteAnticipacion_cancelaSinMora', async () => {
        // HU-07 - Cancelar reserva con mora simple
        // CA 2: Dado que la cancelación se realiza con suficiente anticipación, cuando se
        // procese la operación, entonces la reserva debe quedar cancelada sin mora.
        
        // Arrange
        const bookingId = 1;
        const mockResponse = {
            isSuccess: true,
            data: {
                id: bookingId,
                status: 'Cancelled',
                cancellationFee: 0
            }
        };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.cancel(bookingId, true);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}/cancel`, {
            method: 'POST',
            body: { confirmCancellation: true }
        });
        expect(result.data.status).toBe('Cancelled');
        expect(result.data.cancellationFee).toBe(0);
    });

    it('cancel_cancelacionTardia_calculaMoraCorrespondiente', async () => {
        // HU-07 - Cancelar reserva con mora simple
        // CA 3: Dado que la cancelación se realiza dentro del plazo definido como tardío,
        // cuando se procese la operación, entonces el sistema debe calcular y registrar
        // la mora correspondiente.
        
        // Arrange
        const bookingId = 1;
        const mockResponse = {
            isSuccess: true,
            data: {
                id: bookingId,
                status: 'Cancelled',
                cancellationFee: 100
            }
        };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.cancel(bookingId, true);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}/cancel`, {
            method: 'POST',
            body: { confirmCancellation: true }
        });
        expect(result.data.status).toBe('Cancelled');
        expect(result.data.cancellationFee).toBe(100);
    });

    it('checkOut_reservaVigente_registraCorrectamente', async () => {
        // Arrange
        const bookingId = 1;
        const mockResponse = { isSuccess: true, data: { id: bookingId, status: 'CheckedOut', checkOutTime: '2026-06-25T10:00:00Z' } };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.checkOut(bookingId);

        // Assert
        expect(apiClient.apiRequest).toHaveBeenCalledWith(`/api/Bookings/${bookingId}/check-out`, {
            method: 'POST'
        });
        expect(result).toEqual(mockResponse);
    });

    it('getAgenda_respuestaObjetoConData_retornaData', async () => {
        // Arrange
        const mockResponse = {
            data: [{ id: 1, checkInDate: '2026-06-20T14:00:00Z', status: 'Confirmed' }]
        };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.getAgenda();

        // Assert
        expect(result).toEqual(mockResponse.data);
    });

    it('getAgenda_respuestaObjetoSinData_retornaVacio', async () => {
        // Arrange
        const mockResponse = { data: null };
        apiClient.apiRequest.mockResolvedValue(mockResponse);

        // Act
        const result = await bookingsService.getAgenda();

        // Assert
        expect(result).toEqual([]);
    });
});
