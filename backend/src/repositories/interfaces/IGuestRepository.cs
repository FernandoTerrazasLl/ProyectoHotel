using System.Collections.Generic;
using System.Threading.Tasks;

public interface IGuestRepository
{
    Task<List<Guest>> GetAllAsync();
    Task<Guest?> GetByIdAsync(int id);
    Task<List<Guest>> GetByIdsAsync(IEnumerable<int> ids);
    Task<Guest> AddAsync(Guest guest);
    Task<bool> ExistsByDocumentAsync(string documentType, string documentId, string country, int? excludeGuestId = null);
}
