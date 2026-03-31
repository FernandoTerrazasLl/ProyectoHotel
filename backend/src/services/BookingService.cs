public class BookingService : IBookingService
{
    private const int LateCancellationHoursThreshold = 48;
    private const decimal LateCancellationRate = 0.20m;

    private readonly IBookingRepository _bookingRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IEnumerable<IRoomTypePresetCreator> _roomTypePresetCreators;

    public BookingService(
        IBookingRepository bookingRepository,
        IGuestRepository guestRepository,
        IRoomRepository roomRepository,
        IEnumerable<IRoomTypePresetCreator> roomTypePresetCreators)
    {
        _bookingRepository = bookingRepository;
        _guestRepository = guestRepository;
        _roomRepository = roomRepository;
        _roomTypePresetCreators = roomTypePresetCreators;
    }

    public async Task<OperationResult<BookingSummaryDto>> CreateBookingAsync(CreateBookingRequest request)
    {
        var datesValidation = ValidateCreateDates(request.CheckInDate, request.CheckOutDate);
        if (!datesValidation.IsSuccess)
        {
            return OperationResult<BookingSummaryDto>.Failure(datesValidation.ErrorCode, datesValidation.Message);
        }

        var guestsValidation = ValidateGuestSelection(request);
        if (!guestsValidation.IsSuccess || guestsValidation.Data == null)
        {
            return OperationResult<BookingSummaryDto>.Failure(guestsValidation.ErrorCode, guestsValidation.Message);
        }

        var distinctGuestIds = guestsValidation.Data;

        var guestsExist = await ValidateGuestsExistAsync(distinctGuestIds);
        if (!guestsExist.IsSuccess)
        {
            return OperationResult<BookingSummaryDto>.Failure(guestsExist.ErrorCode, guestsExist.Message);
        }

        var roomValidation = await ValidateRoomForBookingAsync(request.RoomId, request.NumberGuests);
        if (!roomValidation.IsSuccess || roomValidation.Data == null)
        {
            return OperationResult<BookingSummaryDto>.Failure(roomValidation.ErrorCode, roomValidation.Message);
        }

        var checkInDate = datesValidation.Data.CheckInDate;
        var checkOutDate = datesValidation.Data.CheckOutDate;

        var hasOverlap = await _bookingRepository.HasOverlappingBookingAsync(request.RoomId, checkInDate, checkOutDate);
        if (hasOverlap)
        {
            return OperationResult<BookingSummaryDto>.Failure("BOOKING_OVERLAP", "Ya existe una reserva para la habitación en ese rango de fechas.");
        }

        var booking = BuildBooking(request.RoomId, checkInDate, checkOutDate, request.NumberGuests);
        var guestBookings = BuildGuestBookings(distinctGuestIds, request.MainGuestId);

        var created = await _bookingRepository.AddWithGuestsAsync(booking, guestBookings);
        var bookingWithDetails = await _bookingRepository.GetByIdWithDetailsAsync(created.Id);

        if (bookingWithDetails == null)
        {
            return OperationResult<BookingSummaryDto>.Failure("BOOKING_NOT_FOUND", "No se pudo recuperar la reserva creada.");
        }

        return OperationResult<BookingSummaryDto>.Success(MapToSummary(bookingWithDetails));
    }

    public async Task<OperationResult<List<BookingSummaryDto>>> GetActiveAndFutureBookingsAsync()
    {
        var bookings = await _bookingRepository.GetActiveAndFutureAsync(DateTime.Now);
        var summaries = bookings.Select(MapToSummary).ToList();
        return OperationResult<List<BookingSummaryDto>>.Success(summaries);
    }

    public async Task<OperationResult<BookingSummaryDto>> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
        if (booking == null)
        {
            return OperationResult<BookingSummaryDto>.Failure("BOOKING_NOT_FOUND", "La reserva no existe.");
        }

        return OperationResult<BookingSummaryDto>.Success(MapToSummary(booking));
    }

    public async Task<OperationResult<BookingSummaryDto>> CheckInAsync(int bookingId)
    {
        var bookingResult = await GetBookingOrFailureAsync(bookingId);
        if (!bookingResult.IsSuccess || bookingResult.Data == null)
        {
            return OperationResult<BookingSummaryDto>.Failure(bookingResult.ErrorCode, bookingResult.Message);
        }

        var booking = bookingResult.Data;

        var statusValidation = ValidateCheckInStatus(booking);
        if (!statusValidation.IsSuccess)
        {
            return OperationResult<BookingSummaryDto>.Failure(statusValidation.ErrorCode, statusValidation.Message);
        }

        var dateValidation = ValidateCheckInDateWindow(booking);
        if (!dateValidation.IsSuccess)
        {
            return OperationResult<BookingSummaryDto>.Failure(dateValidation.ErrorCode, dateValidation.Message);
        }

        booking.CheckInTime = DateTime.Now;
        booking.Status = BookingStatus.CheckedIn;

        await _bookingRepository.UpdateAsync(booking);

        return await BuildUpdatedSummaryResponseAsync(bookingId, "Check-in registrado correctamente.");
    }

    public async Task<OperationResult<BookingSummaryDto>> CheckOutAsync(int bookingId)
    {
        var bookingResult = await GetBookingOrFailureAsync(bookingId);
        if (!bookingResult.IsSuccess || bookingResult.Data == null)
        {
            return OperationResult<BookingSummaryDto>.Failure(bookingResult.ErrorCode, bookingResult.Message);
        }

        var booking = bookingResult.Data;

        var statusValidation = ValidateCheckOutStatus(booking);
        if (!statusValidation.IsSuccess)
        {
            return OperationResult<BookingSummaryDto>.Failure(statusValidation.ErrorCode, statusValidation.Message);
        }

        booking.CheckOutTime = DateTime.Now;
        booking.Status = BookingStatus.CheckedOut;

        await _bookingRepository.UpdateAsync(booking);

        return await BuildUpdatedSummaryResponseAsync(bookingId, "Check-out registrado correctamente.");
    }

    public async Task<OperationResult<BookingSummaryDto>> CancelBookingAsync(int bookingId, CancelBookingRequest request)
    {
        if (!request.ConfirmCancellation)
        {
            return OperationResult<BookingSummaryDto>.Failure("CANCELLATION_NOT_CONFIRMED", "Debes confirmar la cancelación antes de procesar la operación.");
        }

        var bookingResult = await GetBookingOrFailureAsync(bookingId);
        if (!bookingResult.IsSuccess || bookingResult.Data == null)
        {
            return OperationResult<BookingSummaryDto>.Failure(bookingResult.ErrorCode, bookingResult.Message);
        }

        var booking = bookingResult.Data;

        var statusValidation = ValidateCancelStatus(booking);
        if (!statusValidation.IsSuccess)
        {
            return OperationResult<BookingSummaryDto>.Failure(statusValidation.ErrorCode, statusValidation.Message);
        }

        var room = await _roomRepository.GetByIdWithTypeAsync(booking.RoomId);
        var referencePrice = room?.RoomType?.PricePerNight ?? 0m;

        booking.CancellationFee = CalculateSimpleLateFee(booking.CheckInDate, DateTime.Now, referencePrice);
        booking.Status = BookingStatus.Cancelled;

        await _bookingRepository.UpdateAsync(booking);

        return await BuildUpdatedSummaryResponseAsync(bookingId, "Reserva cancelada correctamente.");
    }

    private async Task<OperationResult<Booking>> GetBookingOrFailureAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null)
        {
            return OperationResult<Booking>.Failure("BOOKING_NOT_FOUND", "La reserva no existe.");
        }

        return OperationResult<Booking>.Success(booking);
    }

    private static OperationResult ValidateCheckInStatus(Booking booking)
    {
        if (booking.Status == BookingStatus.Cancelled)
        {
            return OperationResult.Failure("BOOKING_CANCELLED", "No es posible hacer check-in de una reserva cancelada.");
        }

        if (booking.Status == BookingStatus.CheckedIn)
        {
            return OperationResult.Failure("CHECKIN_ALREADY_DONE", "El check-in ya fue registrado para esta reserva.");
        }

        if (booking.Status == BookingStatus.CheckedOut)
        {
            return OperationResult.Failure("BOOKING_ALREADY_FINISHED", "La reserva ya fue cerrada con check-out.");
        }

        return OperationResult.Success();
    }

    private static OperationResult ValidateCheckInDateWindow(Booking booking)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var checkInDate = DateOnly.FromDateTime(booking.CheckInDate);
        var checkOutDate = DateOnly.FromDateTime(booking.CheckOutDate);

        if (today < checkInDate || today >= checkOutDate)
        {
            return OperationResult.Failure("BOOKING_NOT_CURRENT", "La reserva no está vigente para registrar check-in en esta fecha.");
        }

        return OperationResult.Success();
    }

    private static OperationResult ValidateCheckOutStatus(Booking booking)
    {
        if (booking.Status == BookingStatus.Cancelled)
        {
            return OperationResult.Failure("BOOKING_CANCELLED", "No es posible hacer check-out de una reserva cancelada.");
        }

        if (booking.Status == BookingStatus.CheckedOut)
        {
            return OperationResult.Failure("CHECKOUT_ALREADY_DONE", "El check-out ya fue registrado para esta reserva.");
        }

        if (booking.Status != BookingStatus.CheckedIn)
        {
            return OperationResult.Failure("CHECKOUT_REQUIRES_CHECKIN", "Para registrar check-out, la reserva debe estar en estado checked-in.");
        }

        return OperationResult.Success();
    }

    private static OperationResult ValidateCancelStatus(Booking booking)
    {
        if (booking.Status == BookingStatus.Cancelled)
        {
            return OperationResult.Failure("BOOKING_ALREADY_CANCELLED", "La reserva ya se encuentra cancelada.");
        }

        if (booking.Status == BookingStatus.CheckedIn)
        {
            return OperationResult.Failure("BOOKING_IN_PROGRESS", "No es posible cancelar una reserva con estadía en curso.");
        }

        if (booking.Status == BookingStatus.CheckedOut)
        {
            return OperationResult.Failure("BOOKING_ALREADY_FINISHED", "No es posible cancelar una reserva finalizada.");
        }

        return OperationResult.Success();
    }

    private async Task<OperationResult<BookingSummaryDto>> BuildUpdatedSummaryResponseAsync(int bookingId, string successMessage)
    {
        var updated = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
        if (updated == null)
        {
            return OperationResult<BookingSummaryDto>.Failure("BOOKING_NOT_FOUND", "No se pudo recuperar la reserva actualizada.");
        }

        return OperationResult<BookingSummaryDto>.Success(MapToSummary(updated), successMessage);
    }

    private static OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)> ValidateCreateDates(DateTime rawCheckInDate, DateTime rawCheckOutDate)
    {
        var checkInDate = rawCheckInDate.Date;
        var checkOutDate = rawCheckOutDate.Date;
        var today = DateTime.Today;

        if (checkInDate < today)
        {
            return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Failure("CHECKIN_IN_PAST", "La fecha de ingreso no puede estar en el pasado.");
        }

        if (checkOutDate <= checkInDate)
        {
            return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Failure("INVALID_DATE_RANGE", "La fecha de salida debe ser posterior a la fecha de ingreso.");
        }

        return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Success((checkInDate, checkOutDate));
    }

    private static OperationResult<List<int>> ValidateGuestSelection(CreateBookingRequest request)
    {
        var distinctGuestIds = request.GuestIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (distinctGuestIds.Count == 0)
        {
            return OperationResult<List<int>>.Failure("GUESTS_REQUIRED", "Debes seleccionar al menos un huésped para la reserva.");
        }

        if (distinctGuestIds.Count != request.GuestIds.Count)
        {
            return OperationResult<List<int>>.Failure("DUPLICATE_GUESTS", "La lista de huéspedes contiene IDs repetidos.");
        }

        if (!distinctGuestIds.Contains(request.MainGuestId))
        {
            return OperationResult<List<int>>.Failure("MAIN_GUEST_INVALID", "El huésped principal debe estar incluido en la lista de huéspedes.");
        }

        if (request.NumberGuests != distinctGuestIds.Count)
        {
            return OperationResult<List<int>>.Failure("NUMBER_GUESTS_MISMATCH", "El número de huéspedes debe coincidir con la cantidad de IDs enviados.");
        }

        return OperationResult<List<int>>.Success(distinctGuestIds);
    }

    private async Task<OperationResult> ValidateGuestsExistAsync(List<int> guestIds)
    {
        var guests = await _guestRepository.GetByIdsAsync(guestIds);
        if (guests.Count != guestIds.Count)
        {
            return OperationResult.Failure("GUEST_NOT_FOUND", "Uno o más huéspedes seleccionados no existen.");
        }

        return OperationResult.Success();
    }

    private async Task<OperationResult<Room>> ValidateRoomForBookingAsync(int roomId, int numberGuests)
    {
        var room = await _roomRepository.GetByIdWithTypeAsync(roomId);
        if (room == null)
        {
            return OperationResult<Room>.Failure("ROOM_NOT_FOUND", "La habitación seleccionada no existe.");
        }

        if (!room.IsActive)
        {
            return OperationResult<Room>.Failure("ROOM_NOT_AVAILABLE", "La habitación seleccionada no se encuentra activa.");
        }

        var roomType = room.RoomType;
        if (roomType == null)
        {
            return OperationResult<Room>.Failure("ROOM_TYPE_NOT_FOUND", "La habitación no tiene una variación válida asociada.");
        }

        var preset = ResolvePreset(roomType.Name);
        if (preset == null)
        {
            return OperationResult<Room>.Failure("ROOM_TYPE_NOT_SUPPORTED", "La variación de habitación seleccionada no está soportada por el sistema.");
        }

        var resolvedCapacity = roomType.Capacity > 0 ? roomType.Capacity : preset.Capacity;

        if (numberGuests > resolvedCapacity)
        {
            return OperationResult<Room>.Failure("CAPACITY_EXCEEDED", "La cantidad de personas supera la capacidad de la habitación.");
        }

        return OperationResult<Room>.Success(room);
    }

    private static Booking BuildBooking(int roomId, DateTime checkInDate, DateTime checkOutDate, int numberGuests)
    {
        return new Booking
        {
            RoomId = roomId,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            NumberGuests = numberGuests,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
    }

    private static List<GuestBooking> BuildGuestBookings(IEnumerable<int> guestIds, int mainGuestId)
    {
        return guestIds
            .Select(guestId => new GuestBooking
            {
                GuestId = guestId,
                IsMainGuest = guestId == mainGuestId
            })
            .ToList();
    }

    private static decimal CalculateSimpleLateFee(DateTime checkInDate, DateTime cancellationDate, decimal pricePerNight)
    {
        var hoursBeforeCheckIn = (checkInDate - cancellationDate).TotalHours;
        if (hoursBeforeCheckIn >= LateCancellationHoursThreshold)
        {
            return 0m;
        }

        return Math.Round(pricePerNight * LateCancellationRate, 2, MidpointRounding.AwayFromZero);
    }

    private BookingSummaryDto MapToSummary(Booking booking)
    {
        var guests = booking.GuestBookings
            .Where(gb => gb.Guest != null)
            .Select(gb => new BookingGuestDto
            {
                GuestId = gb.GuestId,
                FirstName = gb.Guest!.FirstName,
                LastName = gb.Guest.LastName,
                FullName = $"{gb.Guest.FirstName} {gb.Guest.LastName}".Trim(),
                DocumentType = gb.Guest.DocumentType,
                DocumentId = gb.Guest.DocumentId,
                IsMainGuest = gb.IsMainGuest
            })
            .OrderByDescending(g => g.IsMainGuest)
            .ThenBy(g => g.GuestId)
            .ToList();

        var mainGuest = guests.FirstOrDefault(g => g.IsMainGuest) ?? guests.FirstOrDefault();
        var roomTypeName = booking.Room?.RoomType?.Name ?? string.Empty;
        var preset = ResolvePreset(roomTypeName);

        var resolvedTypeName = !string.IsNullOrWhiteSpace(roomTypeName)
            ? roomTypeName
            : preset?.TypeName ?? string.Empty;

        var resolvedDescription = !string.IsNullOrWhiteSpace(booking.Room?.RoomType?.Description)
            ? booking.Room!.RoomType!.Description
            : preset?.Description ?? string.Empty;

        var resolvedCapacity = booking.Room?.RoomType?.Capacity ?? 0;
        if (resolvedCapacity <= 0 && preset != null)
        {
            resolvedCapacity = preset.Capacity;
        }

        var resolvedPrice = booking.Room?.RoomType?.PricePerNight ?? 0m;
        if (resolvedPrice <= 0 && preset != null)
        {
            resolvedPrice = preset.ReferencePrice;
        }

        return new BookingSummaryDto
        {
            Id = booking.Id,
            GuestId = mainGuest?.GuestId,
            GuestFullName = mainGuest?.FullName ?? string.Empty,
            MainGuestId = mainGuest?.GuestId,
            MainGuestFullName = mainGuest?.FullName ?? string.Empty,
            Guests = guests,
            RoomId = booking.RoomId,
            RoomNumber = booking.Room?.RoomNumber ?? string.Empty,
            RoomTypeId = booking.Room?.RoomTypeId ?? 0,
            RoomTypeName = resolvedTypeName,
            RoomTypeDescription = resolvedDescription,
            RoomTypeCapacity = resolvedCapacity,
            RoomTypePricePerNight = resolvedPrice,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberGuests = booking.NumberGuests,
            Status = booking.Status,
            CheckInTime = booking.CheckInTime,
            CheckOutTime = booking.CheckOutTime,
            CancellationFee = booking.CancellationFee
        };
    }

    private IRoomTypePresetProduct? ResolvePreset(string roomTypeName)
    {
        var creator = _roomTypePresetCreators.FirstOrDefault(c => c.CanHandle(roomTypeName));
        if (creator == null)
        {
            return null;
        }

        return creator.CreateProduct();
    }
}
