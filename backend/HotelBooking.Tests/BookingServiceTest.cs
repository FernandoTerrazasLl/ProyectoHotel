using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Moq;

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
            CheckOutDate = checkInDate, // Igual
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
        await _context.Guests.AddRangeAsync(
            new Guest { Id = 2, FirstName = "Maria", LastName = "Lopez", DocumentType = "DNI", DocumentId = "87654321", Country = "Bolivia", CreatedAt = DateTime.Now },
            new Guest { Id = 3, FirstName = "Carlos", LastName = "Gomez", DocumentType = "DNI", DocumentId = "11223344", Country = "Bolivia", CreatedAt = DateTime.Now }
        );
        await _context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            GuestIds = new List<int> { 1, 2, 3 },
            MainGuestId = 1,
            RoomId = 1,
            CheckInDate = DateTime.Now.AddDays(2),
            CheckOutDate = DateTime.Now.AddDays(5),
            NumberGuests = 3 // Capacidad de la habitación simple es 2, por lo que 3 la supera
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
    public async Task CreateBookingAsync_SeleccionVariacionHabitacion_AsignaCaracteristicasBase()
    {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 3: Dado que el usuario seleccione una variación de habitación, cuando el
        // sistema procese la selección, entonces debe asignar automáticamente sus
        // características base correspondientes, como capacidad, descripción o precio
        // referencial.

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
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.RoomTypeName, Is.EqualTo("Simple"));
        Assert.That(result.Data.RoomTypeDescription, Is.EqualTo("Habitación simple"));
        Assert.That(result.Data.RoomTypeCapacity, Is.EqualTo(2));
        Assert.That(result.Data.RoomTypePricePerNight, Is.EqualTo(100m));
    }

    [Test]
    public async Task CreateBookingAsync_SinVariacionValida_RetornaFallo()
    {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 5: Dado que se intente registrar una reserva sin una variación válida de
        // habitación, cuando se procese el formulario, entonces el sistema debe
        // impedir el guardado y mostrar una validación.

        // Arrange
        await SeedBaseDataAsync();
        
        var mockRoomRepository = new Moq.Mock<RoomRepository>(_context);
        var roomWithoutType = new Room
        {
            Id = 1,
            RoomNumber = "101",
            RoomTypeId = 1,
            Floor = 1,
            IsActive = true,
            RoomType = null // No valid RoomType
        };
        mockRoomRepository
            .Setup(r => r.GetByIdWithTypeAsync(1))
            .ReturnsAsync(roomWithoutType);

        var serviceWithMock = new BookingService(_bookingRepository, _guestRepository, mockRoomRepository.Object);

        var request = new CreateBookingRequest
        {
            GuestIds = new List<int> { 1 },
            MainGuestId = 1,
            RoomId = 1, // Retornará nuestro roomWithoutType
            CheckInDate = DateTime.Now.AddDays(2),
            CheckOutDate = DateTime.Now.AddDays(5),
            NumberGuests = 1
        };

        // Act
        var result = await serviceWithMock.CreateBookingAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo("ROOM_TYPE_NOT_FOUND"));
    }

    [Test]
    public async Task GetBookingByIdAsync_VisualizaReserva_MuestraInformacionVariacionElegida()
    {
        // HU-05 - Gestionar variación de tipo de habitación en la reserva
        // CA 4: Dado que cada variación tiene características distintas, cuando se visualice la
        // reserva o el resumen de selección, entonces el sistema debe mostrar la
        // información específica de la variación elegida.

        // Arrange
        await SeedBaseDataAsync();
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(2),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        var guestBooking = new GuestBooking
        {
            BookingId = 1,
            GuestId = 1,
            IsMainGuest = true
        };
        await _context.GuestBookings.AddAsync(guestBooking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBookingByIdAsync(1);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.RoomTypeName, Is.EqualTo("Simple"));
        Assert.That(result.Data.RoomTypeDescription, Is.EqualTo("Habitación simple"));
        Assert.That(result.Data.RoomTypeCapacity, Is.EqualTo(2));
        Assert.That(result.Data.RoomTypePricePerNight, Is.EqualTo(100m));
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

    [Test]
    public async Task GetActiveAndFutureBookingsAsync_MultiplesReservas_RetornaOrdenadasCronologicamente()
    {
        // HU-03 - Consultar reservas activas y futuras
        // CA 2: Dado que las reservas tienen fecha de ingreso, cuando se presenten en la
        // lista, entonces deben aparecer ordenadas cronológicamente.

        // Arrange
        await SeedBaseDataAsync();

        var bookingFarFuture = new Booking
        {
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(10),
            CheckOutDate = DateTime.Today.AddDays(15),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(bookingFarFuture);

        var bookingNearFuture = new Booking
        {
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(2),
            CheckOutDate = DateTime.Today.AddDays(5),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(bookingNearFuture);

        await _context.GuestBookings.AddAsync(new GuestBooking { Booking = bookingFarFuture, GuestId = 1, IsMainGuest = true });
        await _context.GuestBookings.AddAsync(new GuestBooking { Booking = bookingNearFuture, GuestId = 1, IsMainGuest = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetActiveAndFutureBookingsAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(2), "Debería retornar ambas reservas");
        
        Assert.That(result.Data[0].CheckInDate, Is.EqualTo(bookingNearFuture.CheckInDate), "La primera reserva debe ser la más cercana cronológicamente");
        Assert.That(result.Data[1].CheckInDate, Is.EqualTo(bookingFarFuture.CheckInDate), "La segunda reserva debe ser la más lejana cronológicamente");
    }

    [Test]
    public async Task GetActiveAndFutureBookingsAsync_SinReservas_RetornaListaVacia()
    {
        // HU-03 - Consultar reservas activas y futuras
        // CA 3: Dado que no existen reservas para mostrar, cuando el usuario abra la vista,
        // entonces el sistema debe informar que no hay datos disponibles.

        // Arrange
        await SeedBaseDataAsync();

        // Act
        var result = await _service.GetActiveAndFutureBookingsAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(0), "La lista de reservas debería estar vacía");
    }

    [Test]
    public async Task CheckInAsync_ReservaVigente_RegistraCorrectamente()
    {
        // HU-04 - Registrar check-in
        // CA 1: Dado que existe una reserva vigente para la fecha correspondiente, cuando el
        // usuario ejecute el check-in, entonces el sistema debe registrar la fecha y hora
        // de ingreso.

        // Arrange
        await SeedBaseDataAsync();

        // 1. Registrar una reserva vigente (iniciando hoy y saliendo en 2 días)
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(2),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        
        // 2. Asociar el huésped
        var guestBooking = new GuestBooking
        {
            BookingId = 1,
            GuestId = 1,
            IsMainGuest = true
        };
        await _context.GuestBookings.AddAsync(guestBooking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CheckInAsync(1);

        // Assert
        Assert.That(result.IsSuccess, Is.True, "El check-in debería registrarse con éxito");
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Status, Is.EqualTo(BookingStatus.CheckedIn), "El estado de la reserva debe cambiar a CheckedIn");
        Assert.That(result.Data.CheckInTime, Is.Not.Null, "La hora de check-in debe registrarse");
        Assert.That(result.Data.CheckInTime!.Value.Date, Is.EqualTo(DateTime.Today), "La fecha de check-in debe ser la de hoy");
    }

    [Test]
    public async Task CheckInAsync_ReservaCancelada_RetornaFallo()
    {
        // HU-04 - Registrar check-in
        // CA 2: Dado que la reserva está cancelada, cuando se intente hacer check-in,
        // entonces el sistema debe impedir la operación.

        // Arrange
        await SeedBaseDataAsync();

        // 1. Registrar una reserva cancelada
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(2),
            NumberGuests = 1,
            Status = BookingStatus.Cancelled,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        
        // 2. Asociar el huésped
        var guestBooking = new GuestBooking
        {
            BookingId = 1,
            GuestId = 1,
            IsMainGuest = true
        };
        await _context.GuestBookings.AddAsync(guestBooking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CheckInAsync(1);

        // Assert
        Assert.That(result.IsSuccess, Is.False, "El check-in debería ser rechazado");
        Assert.That(result.ErrorCode, Is.EqualTo("BOOKING_CANCELLED"), "Debería retornar error de reserva cancelada");
    }

    [Test]
    public async Task CheckInAsync_ReservaYaConCheckIn_RetornaFallo()
    {
        // HU-04 - Registrar check-in
        // CA 3: Dado que una reserva ya realizó check-in, cuando el usuario intente
        // registrarlo nuevamente, entonces el sistema debe evitar duplicar la acción.

        // Arrange
        await SeedBaseDataAsync();

        // 1. Registrar una reserva ya con check-in
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(2),
            NumberGuests = 1,
            Status = BookingStatus.CheckedIn,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        
        // 2. Asociar el huésped
        var guestBooking = new GuestBooking
        {
            BookingId = 1,
            GuestId = 1,
            IsMainGuest = true
        };
        await _context.GuestBookings.AddAsync(guestBooking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CheckInAsync(1);

        // Assert
        Assert.That(result.IsSuccess, Is.False, "El check-in debería ser rechazado");
        Assert.That(result.ErrorCode, Is.EqualTo("CHECKIN_ALREADY_DONE"), "Debería retornar error de check-in ya registrado");
    }

    [Test]
    public async Task CheckInAsync_OperacionExitosa_CambiaEstadoAEstadiaEnCurso()
    {
        // HU-04 - Registrar check-in
        // CA 4: Dado que el check-in fue realizado correctamente, cuando finalice la
        // operación, entonces la reserva debe cambiar a un estado que indique estadía
        // en curso.

        // Arrange
        await SeedBaseDataAsync();
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(2),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        var guestBooking = new GuestBooking
        {
            BookingId = 1,
            GuestId = 1,
            IsMainGuest = true
        };
        await _context.GuestBookings.AddAsync(guestBooking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CheckInAsync(1);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Status, Is.EqualTo(BookingStatus.CheckedIn));
    }

    [Test]
    public async Task CancelBookingAsync_SinConfirmacion_RetornaFallo()
    {
        // HU-07 - Cancelar reserva con mora simple
        // CA 1: Dado que existe una reserva vigente, cuando el usuario seleccione
        // cancelarla, entonces el sistema debe solicitar confirmación antes de aplicar el
        // cambio.

        // Arrange
        await SeedBaseDataAsync();
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = DateTime.Today.AddDays(2),
            CheckOutDate = DateTime.Today.AddDays(5),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var request = new CancelBookingRequest { ConfirmCancellation = false };

        // Act
        var result = await _service.CancelBookingAsync(1, request);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorCode, Is.EqualTo("CANCELLATION_NOT_CONFIRMED"));
    }

    [Test]
    public async Task CancelBookingAsync_SuficienteAnticipacion_CancelaSinMora()
    {
        // HU-07 - Cancelar reserva con mora simple
        // CA 2: Dado que la cancelación se realiza con suficiente anticipación, cuando se
        // procese la operación, entonces la reserva debe quedar cancelada sin mora.

        // Arrange
        await SeedBaseDataAsync();
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = DateTime.Now.AddDays(2), // 48 horas de anticipación
            CheckOutDate = DateTime.Now.AddDays(5),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var request = new CancelBookingRequest { ConfirmCancellation = true };

        // Act
        var result = await _service.CancelBookingAsync(1, request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Status, Is.EqualTo(BookingStatus.Cancelled));
        Assert.That(result.Data.CancellationFee, Is.EqualTo(0m));
    }

    [Test]
    public async Task CancelBookingAsync_CancelacionTardia_CalculaMoraCorrespondiente()
    {
        // HU-07 - Cancelar reserva con mora simple
        // CA 3: Dado que la cancelación se realiza dentro del plazo definido como tardío,
        // cuando se procese la operación, entonces el sistema debe calcular y registrar
        // la mora correspondiente.

        // Arrange
        await SeedBaseDataAsync();
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            CheckInDate = DateTime.Now.AddHours(10), // 10 horas de anticipación (dentro de las 24 horas)
            CheckOutDate = DateTime.Now.AddDays(3),
            NumberGuests = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.Now
        };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        var request = new CancelBookingRequest { ConfirmCancellation = true };

        // Act
        var result = await _service.CancelBookingAsync(1, request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Status, Is.EqualTo(BookingStatus.Cancelled));
        Assert.That(result.Data.CancellationFee, Is.EqualTo(100m)); // 1 noche penalidad = 100m
    }

    private async Task SeedBaseDataAsync()
    {
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

        var roomType = new RoomType
        {
            Id = 1,
            Name = "Simple",
            Description = "Habitación simple",
            Capacity = 2,
            PricePerNight = 100
        };
        await _context.RoomTypes.AddAsync(roomType);

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
