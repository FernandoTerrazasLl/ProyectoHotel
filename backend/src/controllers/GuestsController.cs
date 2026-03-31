using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GuestsController : ControllerBase
{
    private const string GuestNotFoundMessage = "Huésped no encontrado.";

    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Guest>>> GetAll()
    {
        var guests = await _guestService.GetAllGuestsAsync();
        return Ok(guests);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Guest>> GetById(int id)
    {
        var guest = await _guestService.GetGuestByIdAsync(id);
        if (guest == null)
        {
            return NotFound(ApiResponse.Message(GuestNotFoundMessage));
        }

        return Ok(guest);
    }

    [HttpPost]
    public async Task<ActionResult<Guest>> Create([FromBody] GuestRegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _guestService.RegisterGuestAsync(request);
        if (!created.IsSuccess)
        {
            return MapCreateGuestFailure(created);
        }

        return CreatedAtAction(nameof(GetById), new { id = created.Data!.Id }, created.Data);
    }

    private ActionResult<Guest> MapCreateGuestFailure(OperationResult<Guest> result)
    {
        return result.ErrorCode == "DUPLICATE_DOCUMENT"
            ? Conflict(ApiResponse.Message(result.Message))
            : BadRequest(ApiResponse.Message(result.Message));
    }
}
