public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public Task<List<Room>> GetAllActiveRoomsAsync()
    {
        return _roomRepository.GetAllActiveWithTypeAsync();
    }

    public Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime checkInDate, DateTime checkOutDate)
    {
        return _roomRepository.GetAvailableByTypeIdAndDateRangeAsync(roomTypeId, checkInDate, checkOutDate);
    }

    public Task<Room?> GetRoomByIdAsync(int id)
    {
        return _roomRepository.GetByIdWithTypeAsync(id);
    }
}
