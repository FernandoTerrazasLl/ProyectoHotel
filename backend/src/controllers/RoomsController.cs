using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private const string InvalidRoomTypeIdMessage = "El id del tipo de habitación debe ser mayor a 0.";
    private const string PastDatesNotAllowedMessage = "No se permiten fechas en el pasado.";
    private const string InvalidDateRangeMessage = "La fecha de check-out debe ser mayor a la fecha de check-in.";
    private const string NoAvailableRoomsMessage = "No hay habitaciones disponibles para esos parámetros.";
    private const string AvailableRoomsMessage = "Habitaciones disponibles.";
    private const string RoomNotFoundMessage = "Habitación no encontrada.";

    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetAllActive()
    {
        var rooms = await _roomService.GetAllActiveRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] int roomTypeId,
        [FromQuery] DateTime checkInDate,
        [FromQuery] DateTime checkOutDate)
    {
        var validation = ValidateAvailabilityRequest(roomTypeId, checkInDate, checkOutDate);
        if (!validation.IsSuccess)
        {
            return BadRequest(ApiResponse.Message(validation.Message));
        }

        var normalizedDates = validation.Data;

        var rooms = await _roomService.GetAvailableRoomsAsync(
            roomTypeId,
            normalizedDates.CheckInDate,
            normalizedDates.CheckOutDate);

        if (rooms.Count == 0)
        {
            return Ok(ApiResponse.Data(NoAvailableRoomsMessage, rooms));
        }

        return Ok(ApiResponse.Data(AvailableRoomsMessage, rooms));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Room>> GetById(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound(ApiResponse.Message(RoomNotFoundMessage));
        }

        return Ok(room);
    }

    private static OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)> ValidateAvailabilityRequest(
        int roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        if (roomTypeId <= 0)
        {
            return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Failure("INVALID_ROOM_TYPE_ID", InvalidRoomTypeIdMessage);
        }

        var normalizedCheckInDate = checkInDate.Date;
        var normalizedCheckOutDate = checkOutDate.Date;
        var today = DateTime.Today;

        if (normalizedCheckInDate < today || normalizedCheckOutDate < today)
        {
            return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Failure("PAST_DATE", PastDatesNotAllowedMessage);
        }

        if (normalizedCheckInDate >= normalizedCheckOutDate)
        {
            return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Failure("INVALID_DATE_RANGE", InvalidDateRangeMessage);
        }

        return OperationResult<(DateTime CheckInDate, DateTime CheckOutDate)>.Success((normalizedCheckInDate, normalizedCheckOutDate));
    }

}
