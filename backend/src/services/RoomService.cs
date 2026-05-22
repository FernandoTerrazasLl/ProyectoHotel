using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using System.Linq;

public class RoomService
{
    private readonly RoomRepository _roomRepository;
    private readonly BookingRepository _bookingRepository;

    public RoomService(RoomRepository roomRepository, BookingRepository bookingRepository)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<List<Room>> GetAllActiveRoomsAsync()
    {
        var allRooms = await _roomRepository.GetAllWithTypesAsync();
        return allRooms
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoomNumber)
            .ToList();
    }

    public async Task<OperationResult<List<Room>>> GetAvailableRoomsAsync(int roomTypeId, DateTime checkInDate, DateTime checkOutDate)
    {
        var validation = ValidateAvailabilityRequest(roomTypeId, checkInDate, checkOutDate);
        if (!validation.IsSuccess)
        {
            return OperationResult<List<Room>>.Failure(validation.ErrorCode, validation.Message);
        }

        var normalizedDates = validation.Data;
        var normalizedCheckIn = normalizedDates.CheckInDate;
        var normalizedCheckOut = normalizedDates.CheckOutDate;

        var allRooms = await _roomRepository.GetAllWithTypesAsync();
        var allBookings = await _bookingRepository.GetAllAsync();

        var availableRooms = allRooms
            .Where(r => r.IsActive && r.RoomTypeId == roomTypeId)
            .Where(r => !allBookings.Any(b =>
                b.RoomId == r.Id &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.CheckedOut &&
                normalizedCheckIn < b.CheckOutDate &&
                normalizedCheckOut > b.CheckInDate))
            .OrderBy(r => r.RoomNumber)
            .ToList();

        return OperationResult<List<Room>>.Success(availableRooms);
    }

    private static OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)> ValidateAvailabilityRequest(
        int roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        if (roomTypeId <= 0)
        {
            return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Failure("INVALID_ROOM_TYPE_ID", "El id del tipo de habitación debe ser mayor a 0.");
        }

        var normalizedCheckInDate = checkInDate.Date;
        var normalizedCheckOutDate = checkOutDate.Date;
        var today = DateTime.Today;

        if (normalizedCheckInDate < today || normalizedCheckOutDate < today)
        {
            return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Failure("PAST_DATE", "No se permiten fechas en el pasado.");
        }

        if (normalizedCheckInDate >= normalizedCheckOutDate)
        {
            return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Failure("INVALID_DATE_RANGE", "La fecha de check-out debe ser mayor a la fecha de check-in.");
        }

        return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Success((normalizedCheckInDate, normalizedCheckOutDate));
    }

    public Task<Room?> GetRoomByIdAsync(int id)
    {
        return _roomRepository.GetByIdWithTypeAsync(id);
    }
}
