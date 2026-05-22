using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private const string NoAvailableRoomsMessage = "No hay habitaciones disponibles para esos parámetros.";
    private const string AvailableRoomsMessage = "Habitaciones disponibles.";
    private const string RoomNotFoundMessage = "Habitación no encontrada.";

    private readonly RoomService _roomService;

    public RoomsController(RoomService roomService)
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
        var result = await _roomService.GetAvailableRoomsAsync(roomTypeId, checkInDate, checkOutDate);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.Message(result.Message));
        }

        var rooms = result.Data!;

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



}
