using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

[TestFixture]
public class RoomTypeServiceTest
{
    private AppDbContext _context;
    private RoomTypeRepository _repository;
    private RoomTypeService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + System.Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new RoomTypeRepository(_context);
        _service = new RoomTypeService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetRoomTypesAsync_OpcionesDisponibles_RetornaOpcionesValidas()
    {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 1: Dado que el sistema permite registrar una reserva, cuando el usuario
        // seleccione un tipo de habitación, entonces debe poder escoger entre
        // opciones válidas disponibles en el sistema.

        // Arrange
        var roomType1 = new RoomType { Id = 1, Name = "Simple", Description = "Habitación simple", Capacity = 2, PricePerNight = 100 };
        var roomType2 = new RoomType { Id = 2, Name = "Doble", Description = "Habitación doble", Capacity = 4, PricePerNight = 180 };
        await _context.RoomTypes.AddRangeAsync(roomType1, roomType2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRoomTypesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("Simple"));
        Assert.That(result[1].Name, Is.EqualTo("Doble"));
    }
}
