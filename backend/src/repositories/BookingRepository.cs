using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BookingRepository : Repository<Booking>
{
    public BookingRepository(AppDbContext context) : base(context)
    {
    }

    public virtual async Task<Booking?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(b => b.Room)
                .ThenInclude(r => r!.RoomType)
            .Include(b => b.GuestBookings)
                .ThenInclude(gb => gb.Guest)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public virtual async Task<Booking> AddWithGuestsAsync(Booking booking, IEnumerable<GuestBooking> guestBookings)
    {
        foreach (var guestBooking in guestBookings)
        {
            booking.GuestBookings.Add(guestBooking);
        }

        _dbSet.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public virtual async Task<List<Booking>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(b => b.Room)
                .ThenInclude(r => r!.RoomType)
            .Include(b => b.GuestBookings)
                .ThenInclude(gb => gb.Guest)
            .ToListAsync();
    }
}
