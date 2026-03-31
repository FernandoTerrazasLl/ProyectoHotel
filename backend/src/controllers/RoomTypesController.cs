using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypesController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomType>>> GetAll()
    {
        var roomTypes = await _roomTypeService.GetRoomTypesAsync();
        return Ok(roomTypes);
    }
}
