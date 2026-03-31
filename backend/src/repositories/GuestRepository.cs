using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GuestRepository : IGuestRepository
{
    private readonly AppDbContext _context;

    public GuestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Guest>> GetAllAsync()
    {
        return await _context.Guests.AsNoTracking().ToListAsync();
    }

    public async Task<Guest?> GetByIdAsync(int id)
    {
        return await _context.Guests.FindAsync(id);
    }

    public async Task<List<Guest>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var guestIds = ids.Distinct().ToList();

        return await _context.Guests
            .AsNoTracking()
            .Where(g => guestIds.Contains(g.Id))
            .ToListAsync();
    }

    public async Task<Guest> AddAsync(Guest guest)
    {
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();
        return guest;
    }

    public async Task<bool> ExistsByDocumentAsync(string documentType, string documentId, string country, int? excludeGuestId = null)
    {
        return await _context.Guests.AnyAsync(g =>
            g.DocumentType == documentType &&
            g.DocumentId == documentId &&
            g.Country == country &&
            (!excludeGuestId.HasValue || g.Id != excludeGuestId.Value));
    }
}
