using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

public class RoomRepository : Repository<Room>
{
    public RoomRepository(AppDbContext context) : base(context)
    {
    }

    public virtual async Task<List<Room>> GetAllWithTypesAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.RoomType)
            .ToListAsync();
    }

    public virtual async Task<Room?> GetByIdWithTypeAsync(int id)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
