public interface IRoomService
{
    Task<List<Room>> GetAllActiveRoomsAsync();
    Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime checkInDate, DateTime checkOutDate);
    Task<Room?> GetRoomByIdAsync(int id);
}
