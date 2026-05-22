using System.Collections.Generic;
using System.Threading.Tasks;

public class RoomTypeService
{
    private readonly RoomTypeRepository _roomTypeRepository;

    public RoomTypeService(RoomTypeRepository roomTypeRepository)
    {
        _roomTypeRepository = roomTypeRepository;
    }

    public Task<List<RoomType>> GetRoomTypesAsync()
    {
        return _roomTypeRepository.GetAllAsync();
    }
}
