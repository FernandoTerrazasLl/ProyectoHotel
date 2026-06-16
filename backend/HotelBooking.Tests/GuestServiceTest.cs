using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

[TestFixture]
public class GuestServiceTest
{
    private AppDbContext _context;
    private GuestRepository _repository;
    private GuestService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + System.Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new GuestRepository(_context);
        _service = new GuestService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task RegisterGuestAsync_CamposCompletos_RegistroCorrecto()
    {
        // HU-01 - Registrar huésped
        // CA 1: Dado que la recepcionista accede al formulario de registro, cuando complete
        // los campos obligatorios y guarde, entonces el sistema debe registrar
        // correctamente al huésped.
        
        // Arrange
        var request = CreateValidRegistrationRequest();

        // Act
        var result = await _service.RegisterGuestAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True, "El resultado debería ser exitoso");
        Assert.That(result.Message, Is.EqualTo("Huésped registrado correctamente."));
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.FirstName, Is.EqualTo("Juan"));
        
        var guestsInDb = await _repository.GetAllAsync();
        Assert.That(guestsInDb.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task RegisterGuestAsync_CamposIncompletos_RetornaFallo()
    {
        // HU-01 - Registrar huésped
        // CA 2: Dado que falta uno o más campos obligatorios, cuando intente guardar el
        // formulario, entonces el sistema debe mostrar validaciones y no registrar el
        // huésped.
        
        // Arrange
        var request = CreateValidRegistrationRequest();
        request.FirstName = ""; // Campo obligatorio vacío

        // Act
        var result = await _service.RegisterGuestAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.False, "El registro debería fallar por campos incompletos");
        Assert.That(result.ErrorCode, Is.EqualTo("MISSING_REQUIRED_FIELDS"));
        
        var guestsInDb = await _repository.GetAllAsync();
        Assert.That(guestsInDb.Count, Is.EqualTo(0), "No debería registrarse ningún huésped en la base de datos");
    }

    private static GuestRegistrationRequest CreateValidRegistrationRequest()
    {
        return new GuestRegistrationRequest
        {
            FirstName = "Juan",
            LastName = "Perez",
            DocumentType = "DNI",
            DocumentId = "12345678",
            Country = "Bolivia",
            Email = "juan@example.com",
            Phone = "12345678"
        };
    }
}
