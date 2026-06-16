using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

[TestFixture]
public class BookingServiceTest
{
    private AppDbContext _context;
    private BookingRepository _bookingRepository;
    private GuestRepository _guestRepository;
    private RoomRepository _roomRepository;
    private BookingService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _bookingRepository = new BookingRepository(_context);
        _guestRepository = new GuestRepository(_context);
        _roomRepository = new RoomRepository(_context);
        _service = new BookingService(_bookingRepository, _guestRepository, _roomRepository);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task CreateBookingAsync_CamposCompletos_RegistroCorrecto()
    {
        // HU-02 - Crear reserva de habitación
        // CA 1: Dado que existen huéspedes y habitaciones precargadas, cuando el usuario
        // complete los datos requeridos de la reserva, entonces el sistema debe
        // registrarla correctamente.

        // Arrange
        await SeedBaseDataAsync();

        var request = new CreateBookingRequest
        {
            GuestIds = new List<int> { 1 },
            MainGuestId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Now.AddDays(2),
            CheckOutDate = DateTime.Now.AddDays(5),
            NumberGuests = 1
        };

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True, "La reserva debería ser creada con éxito");
        Assert.That(result.Data, Is.Not.Null, "Los datos de la reserva no deberían ser nulos");
        Assert.That(result.Data.RoomNumber, Is.EqualTo("101"), "La habitación asignada debería ser 101");
        
        var bookingsInDb = await _bookingRepository.GetAllAsync();
        Assert.That(bookingsInDb.Count, Is.EqualTo(1), "Debería haber exactamente 1 reserva en la base de datos");
    }

    [Test]
    public async Task CreateBookingAsync_FechaSalidaNoPosterior_RetornaFallo()
    {
        // HU-02 - Crear reserva de habitación
        // CA 2: Dado que la fecha de salida no es posterior a la fecha de ingreso, cuando se
        // intente guardar la reserva, entonces el sistema debe impedir el registro y
        // mostrar una validación.

        // Arrange
        await SeedBaseDataAsync();

        var checkInDate = DateTime.Now.AddDays(2);
        var request = new CreateBookingRequest
        {
            GuestIds = new List<int> { 1 },
            MainGuestId = 1,
            RoomId = 1,
            CheckInDate = checkInDate,
            CheckOutDate = checkInDate, // Igual a la fecha de ingreso (no posterior)
            NumberGuests = 1
        };

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.False, "El registro debería fallar");
        Assert.That(result.ErrorCode, Is.EqualTo("INVALID_DATE_RANGE"), "Debería retornar error de rango de fechas inválido");
        
        var bookingsInDb = await _bookingRepository.GetAllAsync();
        Assert.That(bookingsInDb.Count, Is.EqualTo(0), "No debería guardarse ninguna reserva en la base de datos");
    }

    [Test]
    public async Task CreateBookingAsync_ReservaSolapada_RetornaFallo()
    {
        // HU-02 - Crear reserva de habitación
        // CA 3: Dado que una habitación ya está reservada en el mismo rango de fechas,
        // cuando se intente registrar una nueva reserva para esa habitación, entonces
        // el sistema debe impedir el solapamiento.

        // Arrange
        await SeedBaseDataAsync();

        var checkInDate = DateTime.Now.AddDays(2);
        var checkOutDate = DateTime.Now.AddDays(5);

        // 1. Crear primera reserva en la base de datos
        var existingBooking = new Booking
        {
            RoomId = 1,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(existingBooking);
        await _context.SaveChangesAsync();

        // 2. Intentar crear una segunda reserva para la misma habitación en el mismo rango
        var request = new CreateBookingRequest
        {
            GuestIds = new List<int> { 1 },
            MainGuestId = 1,
            RoomId = 1,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            NumberGuests = 1
        };

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.False, "El registro debería fallar por solapamiento");
        Assert.That(result.ErrorCode, Is.EqualTo("BOOKING_OVERLAP"), "Debería retornar error de solapamiento de reserva");
        
        var bookingsInDb = await _bookingRepository.GetAllAsync();
        Assert.That(bookingsInDb.Count, Is.EqualTo(1), "Debería mantenerse únicamente la primera reserva en la base de datos");
    }

    [Test]
    public async Task CreateBookingAsync_SuperaCapacidadHabitacion_RetornaFallo()
    {
        // HU-02 - Crear reserva de habitación
        // CA 4: Dado que la cantidad de personas supera la capacidad de la habitación,
        // cuando se intente guardar la reserva, entonces el sistema debe rechazar la
        // operación.

        // Arrange
        await SeedBaseDataAsync();

        var request = new CreateBookingRequest
        {
            GuestIds = new List<int> { 1 },
            MainGuestId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Now.AddDays(2),
            CheckOutDate = DateTime.Now.AddDays(5),
            NumberGuests = 5 // Capacidad de la habitación simple es 2, por lo que 5 la supera
        };

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.False, "El registro debería fallar por exceder capacidad");
        Assert.That(result.ErrorCode, Is.EqualTo("CAPACITY_EXCEEDED"), "Debería retornar error de capacidad excedida");
        
        var bookingsInDb = await _bookingRepository.GetAllAsync();
        Assert.That(bookingsInDb.Count, Is.EqualTo(0), "No debería guardarse ninguna reserva en la base de datos");
    }

    [Test]
    public async Task GetActiveAndFutureBookingsAsync_ReservasRegistradas_RetornaReservas()
    {
        // HU-03 - Consultar reservas activas y futuras
        // CA 1: Dado que existen reservas registradas, cuando el usuario ingrese al listado,
        // entonces el sistema debe mostrar las reservas activas y futuras con sus datos
        // principales.

        // Arrange
        await SeedBaseDataAsync();

        // 1. Registrar una reserva en el futuro
        var booking = new Booking
        {
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(2),
            CheckOutDate = DateTime.Today.AddDays(5),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        
        // 2. Asociar el huésped a la reserva
        var guestBooking = new GuestBooking
        {
            Booking = booking,
            GuestId = 1,
            IsMainGuest = true
        };
        await _context.GuestBookings.AddAsync(guestBooking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetActiveAndFutureBookingsAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True, "Debería retornar exitoso");
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(1), "Debería retornar exactamente 1 reserva en la agenda");
        Assert.That(result.Data[0].RoomNumber, Is.EqualTo("101"), "La habitación listada debería ser 101");
        Assert.That(result.Data[0].MainGuestFullName, Is.EqualTo("Juan Perez"), "El huésped principal debería ser Juan Perez");
    }

    private async Task SeedBaseDataAsync()
    {
        // 1. Precargar Huésped
        var guest = new Guest
        {
            Id = 1,
            FirstName = "Juan",
            LastName = "Perez",
            DocumentType = "DNI",
            DocumentId = "12345678",
            Country = "Bolivia",
            CreatedAt = DateTime.Now
        };
        await _context.Guests.AddAsync(guest);

        // 2. Precargar Tipo de Habitación
        var roomType = new RoomType
        {
            Id = 1,
            Name = "Simple",
            Description = "Habitación simple",
            Capacity = 2,
            PricePerNight = 100
        };
        await _context.RoomTypes.AddAsync(roomType);

        // 3. Precargar Habitación
        var room = new Room
        {
            Id = 1,
            RoomNumber = "101",
            RoomTypeId = 1,
            Floor = 1,
            IsActive = true
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
    }
}
