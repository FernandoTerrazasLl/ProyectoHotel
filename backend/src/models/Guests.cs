public class Guest
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Country { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<GuestBooking> GuestBookings { get; set; } = new List<GuestBooking>();
}
