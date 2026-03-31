public interface IServiceContactRepository
{
    Task<List<ServiceContact>> GetAllAsync();
}
