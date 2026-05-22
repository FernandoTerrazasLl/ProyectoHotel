using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

[TestFixture]
public class BookingServiceTests
{
    private Mock<BookingRepository> _mockBookingRepository = null!;
    private Mock<GuestRepository> _mockGuestRepository = null!;
    private Mock<RoomRepository> _mockRoomRepository = null!;
    private BookingService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        
        var mockBookingDbSet = new Mock<DbSet<Booking>>();
        var mockGuestDbSet = new Mock<DbSet<Guest>>();
        var mockRoomDbSet = new Mock<DbSet<Room>>();
        
        mockContext.Setup(c => c.Set<Booking>()).Returns(mockBookingDbSet.Object);
        mockContext.Setup(c => c.Set<Guest>()).Returns(mockGuestDbSet.Object);
        mockContext.Setup(c => c.Set<Room>()).Returns(mockRoomDbSet.Object);

        _mockBookingRepository = new Mock<BookingRepository>(mockContext.Object);
        _mockGuestRepository = new Mock<GuestRepository>(mockContext.Object);
        _mockRoomRepository = new Mock<RoomRepository>(mockContext.Object);
        
        _service = new BookingService(
            _mockBookingRepository.Object,
            _mockGuestRepository.Object,
            _mockRoomRepository.Object
        );
    }

    [Test]
    public async Task CreateBookingAsync_SuperaCapacidadHabitacion_ImpideReserva()
    {
        // HU-02 - Criterio 4: Dado que la cantidad de personas supera la capacidad de la habitación, 
        // cuando se intente guardar la reserva, entonces el sistema debe rechazar la operación.

        // Arrange
        var checkInDate = DateTime.Today.AddDays(1);
        var checkOutDate = DateTime.Today.AddDays(3);

        var request = new CreateBookingRequest
        {
            RoomId = 101,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            MainGuestId = 1,
            GuestIds = new List<int> { 1, 2, 3 }, // 3 huéspedes
            NumberGuests = 3
        };

        var mockRoom = new Room
        {
            Id = 101,
            RoomNumber = "101",
            IsActive = true,
            RoomTypeId = 1,
            RoomType = new RoomType
            {
                Id = 1,
                Name = "Simple",
                Capacity = 2, // Capacidad máxima: 2 personas (menos que los 3 solicitados)
                PricePerNight = 100m
            }
        };

        // Mock checking guest existence: we return 3 existing guests
        var allGuests = new List<Guest>
        {
            new Guest { Id = 1 },
            new Guest { Id = 2 },
            new Guest { Id = 3 }
        };
        _mockGuestRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allGuests);

        // Mock RoomRepository to return the room with simple type (capacity 2)
        _mockRoomRepository.Setup(r => r.GetByIdWithTypeAsync(101)).ReturnsAsync(mockRoom);

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo("CAPACITY_EXCEEDED"));
        Assert.That(result.Message, Is.EqualTo("La cantidad de personas supera la capacidad de la habitación."));
        _mockBookingRepository.Verify(r => r.AddWithGuestsAsync(It.IsAny<Booking>(), It.IsAny<IEnumerable<GuestBooking>>()), Times.Never);
    }
}
