using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RoomTypeRepository : Repository<RoomType>
{
    public RoomTypeRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<List<RoomType>> GetAllAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(rt => rt.PricePerNight)
            .ToListAsync();
    }
}
