public class BookingGuestDto
{
    public int GuestId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public bool IsMainGuest { get; set; }
}
