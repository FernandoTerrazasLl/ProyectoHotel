public class ServiceContactService : IServiceContactService
{
    private readonly IServiceContactRepository _serviceContactRepository;

    public ServiceContactService(IServiceContactRepository serviceContactRepository)
    {
        _serviceContactRepository = serviceContactRepository;
    }

    public Task<List<ServiceContact>> GetAllContactsAsync()
    {
        return _serviceContactRepository.GetAllAsync();
    }
}
