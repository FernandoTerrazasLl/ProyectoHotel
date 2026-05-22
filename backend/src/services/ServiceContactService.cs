using System.Collections.Generic;
using System.Threading.Tasks;

public class ServiceContactService
{
    private readonly ServiceContactRepository _serviceContactRepository;

    public ServiceContactService(ServiceContactRepository serviceContactRepository)
    {
        _serviceContactRepository = serviceContactRepository;
    }

    public Task<List<ServiceContact>> GetAllContactsAsync()
    {
        return _serviceContactRepository.GetAllAsync();
    }
}
