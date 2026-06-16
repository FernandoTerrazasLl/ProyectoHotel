using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

[TestFixture]
public class RoomServiceTest
{
    private AppDbContext _context;
    private RoomRepository _roomRepository;
    private BookingRepository _bookingRepository;
    private RoomService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _roomRepository = new RoomRepository(_context);
        _bookingRepository = new BookingRepository(_context);
        _service = new RoomService(_roomRepository, _bookingRepository);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetAllActiveRoomsAsync_OpcionesDisponibles_RetornaSoloHabitacionesActivasOrdenadas()
    {
        // Arrange
        var roomType = new RoomType { Id = 1, Name = "Simple", Description = "Simple Room", Capacity = 2, PricePerNight = 100 };
        await _context.RoomTypes.AddAsync(roomType);

        var roomActive2 = new Room { Id = 2, RoomNumber = "102", RoomTypeId = 1, Floor = 1, IsActive = true };
        var roomActive1 = new Room { Id = 1, RoomNumber = "101", RoomTypeId = 1, Floor = 1, IsActive = true };
        var roomInactive = new Room { Id = 3, RoomNumber = "103", RoomTypeId = 1, Floor = 1, IsActive = false };

        await _context.Rooms.AddRangeAsync(roomActive2, roomActive1, roomInactive);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllActiveRoomsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].RoomNumber, Is.EqualTo("101"));
        Assert.That(result[1].RoomNumber, Is.EqualTo("102"));
    }

    [Test]
    public async Task GetRoomByIdAsync_HabitacionExistente_RetornaHabitacion()
    {
        // Arrange
        var roomType = new RoomType { Id = 1, Name = "Simple", Description = "Simple Room", Capacity = 2, PricePerNight = 100 };
        await _context.RoomTypes.AddAsync(roomType);

        var room = new Room { Id = 1, RoomNumber = "101", RoomTypeId = 1, Floor = 1, IsActive = true };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRoomByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.RoomNumber, Is.EqualTo("101"));
    }

    [Test]
    public async Task GetAvailableRoomsAsync_RequestValido_RetornaHabitacionesDisponibles()
    {
        // Arrange
        var roomType = new RoomType { Id = 1, Name = "Simple", Description = "Simple Room", Capacity = 2, PricePerNight = 100 };
        await _context.RoomTypes.AddAsync(roomType);

        var room1 = new Room { Id = 1, RoomNumber = "101", RoomTypeId = 1, Floor = 1, IsActive = true };
        var room2 = new Room { Id = 2, RoomNumber = "102", RoomTypeId = 1, Floor = 1, IsActive = true };
        await _context.Rooms.AddRangeAsync(room1, room2);

        var checkInDate = DateTime.Today.AddDays(2);
        var checkOutDate = DateTime.Today.AddDays(5);

        // Create an overlapping booking for Room 1
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAvailableRoomsAsync(1, checkInDate, checkOutDate);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(1));
        Assert.That(result.Data[0].RoomNumber, Is.EqualTo("102"));
    }

    [Test]
    public async Task GetAvailableRoomsAsync_RoomTypeIdInvalido_RetornaFallo()
    {
        // Arrange
        var checkInDate = DateTime.Today.AddDays(2);
        var checkOutDate = DateTime.Today.AddDays(5);

        // Act
        var result = await _service.GetAvailableRoomsAsync(0, checkInDate, checkOutDate);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo(RoomService.InvalidRoomTypeIdCode));
    }

    [Test]
    public async Task GetAvailableRoomsAsync_FechaPasada_RetornaFallo()
    {
        // Arrange
        var checkInDate = DateTime.Today.AddDays(-2);
        var checkOutDate = DateTime.Today.AddDays(2);

        // Act
        var result = await _service.GetAvailableRoomsAsync(1, checkInDate, checkOutDate);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo(RoomService.PastDateCode));
    }

    [Test]
    public async Task GetAvailableRoomsAsync_RangoFechasInvalido_RetornaFallo()
    {
        // Arrange
        var checkInDate = DateTime.Today.AddDays(5);
        var checkOutDate = DateTime.Today.AddDays(2);

        // Act
        var result = await _service.GetAvailableRoomsAsync(1, checkInDate, checkOutDate);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo(RoomService.InvalidDateRangeCode));
    }
}
