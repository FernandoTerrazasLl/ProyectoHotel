public interface IRoomTypeRepository
{
    Task<List<RoomType>> GetAllAsync();
}
