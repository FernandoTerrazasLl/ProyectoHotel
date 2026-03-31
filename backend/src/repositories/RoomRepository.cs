using Microsoft.EntityFrameworkCore;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Room>> GetAllActiveWithTypeAsync()
    {
        return await _context.Rooms
            .AsNoTracking()
            .Include(r => r.RoomType)
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<List<Room>> GetAvailableByTypeIdAndDateRangeAsync(int roomTypeId, DateTime checkInDate, DateTime checkOutDate)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Include(r => r.RoomType)
            .Where(r =>
                r.IsActive &&
                r.RoomTypeId == roomTypeId &&
                !_context.Bookings.Any(b =>
                        b.RoomId == r.Id &&
                        b.Status != BookingStatus.Cancelled &&
                        b.Status != BookingStatus.CheckedOut &&
                        checkInDate < b.CheckOutDate &&
                        checkOutDate > b.CheckInDate))
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();
    }

    public async Task<Room?> GetByIdWithTypeAsync(int id)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
