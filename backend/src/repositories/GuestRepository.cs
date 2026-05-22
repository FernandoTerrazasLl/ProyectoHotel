using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GuestRepository : Repository<Guest>
{
    public GuestRepository(AppDbContext context) : base(context)
    {
    }

}
