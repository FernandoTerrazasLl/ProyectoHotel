using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private const string BookingNotFoundMessage = "Reserva no encontrada.";
    private const string EmptyAgendaMessage = "No hay datos disponibles.";
    private const string AgendaSuccessMessage = "Reservas activas y futuras.";

    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _bookingService.CreateBookingAsync(request);
        if (!result.IsSuccess)
        {
            return MapCreateBookingFailure(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _bookingService.GetBookingByIdAsync(id);
        if (!result.IsSuccess)
        {
            return result.ErrorCode == "BOOKING_NOT_FOUND"
                ? NotFound(ApiResponse.Message(BookingNotFoundMessage))
                : BadRequest(ApiResponse.Message(result.Message));
        }

        return Ok(result.Data);
    }

    [HttpGet("agenda")]
    public async Task<IActionResult> GetActiveAndFuture()
    {
        var result = await _bookingService.GetActiveAndFutureBookingsAsync();
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.Message(result.Message));
        }

        if (result.Data == null || result.Data.Count == 0)
        {
            return Ok(ApiResponse.Data(EmptyAgendaMessage, new List<BookingSummaryDto>()));
        }

        return Ok(ApiResponse.Data(AgendaSuccessMessage, result.Data));
    }

    [HttpPost("{id:int}/check-in")]
    public async Task<IActionResult> CheckIn(int id)
    {
        var result = await _bookingService.CheckInAsync(id);
        if (!result.IsSuccess)
        {
            return MapBookingOperationFailure(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:int}/check-out")]
    public async Task<IActionResult> CheckOut(int id)
    {
        var result = await _bookingService.CheckOutAsync(id);
        if (!result.IsSuccess)
        {
            return MapBookingOperationFailure(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelBookingRequest request)
    {
        var result = await _bookingService.CancelBookingAsync(id, request);
        if (!result.IsSuccess)
        {
            return MapBookingOperationFailure(result);
        }

        return Ok(result.Data);
    }

    private IActionResult MapCreateBookingFailure(OperationResult<BookingSummaryDto> result)
    {
        return result.ErrorCode switch
        {
            "GUEST_NOT_FOUND" => NotFound(ApiResponse.Message(result.Message)),
            "ROOM_NOT_FOUND" => NotFound(ApiResponse.Message(result.Message)),
            "ROOM_TYPE_NOT_FOUND" => BadRequest(ApiResponse.Message(result.Message)),
            "BOOKING_OVERLAP" => Conflict(ApiResponse.Message(result.Message)),
            _ => BadRequest(ApiResponse.Message(result.Message))
        };
    }

    private IActionResult MapBookingOperationFailure(OperationResult<BookingSummaryDto> result)
    {
        return result.ErrorCode == "BOOKING_NOT_FOUND"
            ? NotFound(ApiResponse.Message(result.Message))
            : BadRequest(ApiResponse.Message(result.Message));
    }
}
