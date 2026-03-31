public interface IServiceContactService
{
    Task<List<ServiceContact>> GetAllContactsAsync();
}
