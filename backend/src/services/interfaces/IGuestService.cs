public interface IGuestService
{
    Task<IEnumerable<Guest>> GetAllGuestsAsync();
    Task<Guest?> GetGuestByIdAsync(int id);
    Task<OperationResult<Guest>> RegisterGuestAsync(GuestRegistrationRequest request);
}
