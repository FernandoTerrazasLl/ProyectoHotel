using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ServiceContactsController : ControllerBase
{
    private const string EmptyContactsMessage = "No hay contactos disponibles.";
    private const string ContactsSuccessMessage = "Contactos de servicios del hotel.";

    private readonly IServiceContactService _serviceContactService;

    public ServiceContactsController(IServiceContactService serviceContactService)
    {
        _serviceContactService = serviceContactService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var contacts = await _serviceContactService.GetAllContactsAsync();
        if (contacts.Count == 0)
        {
            return Ok(ApiResponse.Data(EmptyContactsMessage, contacts));
        }

        return Ok(ApiResponse.Data(ContactsSuccessMessage, contacts));
    }
}
