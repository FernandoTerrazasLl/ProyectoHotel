using Microsoft.EntityFrameworkCore;

public class ServiceContactRepository : IServiceContactRepository
{
    private readonly AppDbContext _context;

    public ServiceContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceContact>> GetAllAsync()
    {
        return await _context.ServiceContacts
            .AsNoTracking()
            .OrderBy(sc => sc.ServiceName)
            .ToListAsync();
    }
}
