public class GuestBooking
{
    public int Id { get; set; }
    public int GuestId { get; set; }
    public int BookingId { get; set; }
    public bool IsMainGuest { get; set; }

    public Guest? Guest { get; set; }
    public Booking? Booking { get; set; }
}
