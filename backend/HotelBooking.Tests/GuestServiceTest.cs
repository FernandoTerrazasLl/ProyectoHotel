using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

[TestFixture]
public class GuestServiceTest
{
    private Mock<GuestRepository> _mockRepository = null!;
    private GuestService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        var mockDbSet = new Mock<DbSet<Guest>>();
        mockContext.Setup(c => c.Set<Guest>()).Returns(mockDbSet.Object);

        _mockRepository = new Mock<GuestRepository>(mockContext.Object);
        _service = new GuestService(_mockRepository.Object);
    }

    [Test]
    public async Task RegisterGuestAsync_DatosValidos_RegistraExitosamente()
    {
        // HU-01 - Criterio 1: Dado que la recepcionista accede al formulario de registro, 
        // cuando complete los campos obligatorios y guarde, entonces el sistema debe registrar correctamente al huésped.
        
        // Arrange
        var request = new GuestRegistrationRequest
        {
            FirstName = "Juan",
            LastName = "Perez",
            DocumentType = "CI",
            DocumentId = "123456",
            Country = "Bolivia",
            Email = "juan.perez@example.com",
            Phone = "77777777"
        };

        var expectedGuest = new Guest
        {
            Id = 1,
            FirstName = "Juan",
            LastName = "Perez",
            DocumentType = "CI",
            DocumentId = "123456",
            Country = "Bolivia",
            Email = "juan.perez@example.com",
            Phone = "77777777"
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Guest>());
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Guest>()))
            .ReturnsAsync(expectedGuest);

        // Act
        var result = await _service.RegisterGuestAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.FirstName, Is.EqualTo("Juan"));
        Assert.That(result.Data.LastName, Is.EqualTo("Perez"));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Guest>()), Times.Once);
    }

    [Test]
    public async Task RegisterGuestAsync_HuespedDuplicado_ImpideRegistro()
    {
        // HU-01 - Criterio 3: Dado que ya existe un huésped con el mismo documento de identidad, 
        // cuando se intente registrar nuevamente, entonces el sistema debe impedir el duplicado.

        // Arrange
        var request = new GuestRegistrationRequest
        {
            FirstName = "Pedro",
            LastName = "Gomez",
            DocumentType = "CI",
            DocumentId = "123456", // Documento duplicado
            Country = "Bolivia"
        };

        var existingGuests = new List<Guest>
        {
            new Guest
            {
                Id = 1,
                FirstName = "Juan",
                LastName = "Perez",
                DocumentType = "CI",
                DocumentId = "123456",
                Country = "Bolivia"
            }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(existingGuests);

        // Act
        var result = await _service.RegisterGuestAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo("DUPLICATE_DOCUMENT"));
        Assert.That(result.Message, Is.EqualTo("Ya existe un huésped con el mismo tipo y número de documento en ese país."));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Guest>()), Times.Never);
    }
}
