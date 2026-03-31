using Microsoft.EntityFrameworkCore;

public class RoomTypeRepository : IRoomTypeRepository
{
    private readonly AppDbContext _context;

    public RoomTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoomType>> GetAllAsync()
    {
        return await _context.RoomTypes
            .AsNoTracking()
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();
    }
}
