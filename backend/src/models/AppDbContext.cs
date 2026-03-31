using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Guest> Guests { get; set; } = null!;
    public DbSet<ServiceContact> ServiceContacts { get; set; } = null!;
    public DbSet<RoomType> RoomTypes { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<GuestBooking> GuestBookings { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<BookingStatus>("booking_status");

        ConfigureGuest(modelBuilder.Entity<Guest>());
        ConfigureRoom(modelBuilder.Entity<Room>());
        ConfigureRoomType(modelBuilder.Entity<RoomType>());
        ConfigureBooking(modelBuilder.Entity<Booking>());
        ConfigureGuestBooking(modelBuilder.Entity<GuestBooking>());
        ConfigureServiceContact(modelBuilder.Entity<ServiceContact>());

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureGuest(EntityTypeBuilder<Guest> entity)
    {
        entity.HasIndex(e => new { e.DocumentType, e.DocumentId, e.Country }).IsUnique();
        entity.Property(e => e.Email).IsRequired(false);
        entity.Property(e => e.Phone).IsRequired(false);
        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .HasDefaultValueSql("now()");
    }

    private static void ConfigureRoom(EntityTypeBuilder<Room> entity)
    {
        entity.HasIndex(e => e.RoomNumber).IsUnique();
        entity.HasOne(e => e.RoomType)
            .WithMany(t => t.Rooms)
            .HasForeignKey(e => e.RoomTypeId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureRoomType(EntityTypeBuilder<RoomType> entity)
    {
        entity.HasIndex(e => e.Name).IsUnique();
    }

    private static void ConfigureBooking(EntityTypeBuilder<Booking> entity)
    {
        entity.ToTable("bookings");

        entity.Property(e => e.CheckInDate)
            .HasColumnType("date");
        entity.Property(e => e.CheckOutDate)
            .HasColumnType("date");
        entity.Property(e => e.CheckInTime)
            .HasColumnType("timestamp without time zone");
        entity.Property(e => e.CheckOutTime)
            .HasColumnType("timestamp without time zone");
        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp without time zone");
        entity.Property(e => e.Status)
            .HasColumnType("booking_status");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        entity.HasOne(e => e.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureGuestBooking(EntityTypeBuilder<GuestBooking> entity)
    {
        entity.ToTable("guests_booking");

        entity.HasIndex(e => new { e.GuestId, e.BookingId }).IsUnique();

        entity.HasOne(e => e.Guest)
            .WithMany(g => g.GuestBookings)
            .HasForeignKey(e => e.GuestId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.Booking)
            .WithMany(b => b.GuestBookings)
            .HasForeignKey(e => e.BookingId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureServiceContact(EntityTypeBuilder<ServiceContact> entity)
    {
        entity.Property(e => e.ServiceName).IsRequired();
        entity.Property(e => e.Responsible).IsRequired();
        entity.Property(e => e.Phone).IsRequired();
    }
}