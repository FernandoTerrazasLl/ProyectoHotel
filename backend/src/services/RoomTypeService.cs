public class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _roomTypeRepository;

    public RoomTypeService(IRoomTypeRepository roomTypeRepository)
    {
        _roomTypeRepository = roomTypeRepository;
    }

    public Task<List<RoomType>> GetRoomTypesAsync()
    {
        return _roomTypeRepository.GetAllAsync();
    }
}
