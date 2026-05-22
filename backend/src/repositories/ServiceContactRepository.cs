using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ServiceContactRepository : Repository<ServiceContact>
{
    public ServiceContactRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<List<ServiceContact>> GetAllAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(sc => sc.ServiceName)
            .ToListAsync();
    }
}
