using Microsoft.EntityFrameworkCore;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings.FindAsync(id);
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Room)
                .ThenInclude(r => r!.RoomType)
            .Include(b => b.GuestBookings)
                .ThenInclude(gb => gb.Guest)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking> AddWithGuestsAsync(Booking booking, IEnumerable<GuestBooking> guestBookings)
    {
        foreach (var guestBooking in guestBookings)
        {
            booking.GuestBookings.Add(guestBooking);
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Booking>> GetActiveAndFutureAsync(DateTime referenceDate)
    {
        var referenceDay = referenceDate.Date;

        return await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.CheckedOut &&
                b.CheckOutDate.Date >= referenceDay)
            .Include(b => b.Room)
                .ThenInclude(r => r!.RoomType)
            .Include(b => b.GuestBookings)
                .ThenInclude(gb => gb.Guest)
            .OrderBy(b => b.CheckInDate)
            .ThenBy(b => b.Id)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingBookingAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, int? excludeBookingId = null)
    {
        return await _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.CheckedOut &&
                (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value) &&
                checkInDate < b.CheckOutDate &&
                checkOutDate > b.CheckInDate);
    }
}
