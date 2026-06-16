using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GuestRepository : Repository<Guest>
{
    public GuestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsWithDocumentAsync(string documentType, string documentId, string country)
    {
        var typeTrim = documentType.Trim();
        var idTrim = documentId.Trim();
        var countryTrim = country.Trim();
        
        return await _dbSet.AnyAsync(g =>
            g.DocumentType == typeTrim &&
            g.DocumentId == idTrim &&
            g.Country == countryTrim);
    }
}
