public interface IRoomRepository
{
    Task<List<Room>> GetAllActiveWithTypeAsync();
    Task<List<Room>> GetAvailableByTypeIdAndDateRangeAsync(int roomTypeId, DateTime checkInDate, DateTime checkOutDate);
    Task<Room?> GetByIdWithTypeAsync(int id);
}
