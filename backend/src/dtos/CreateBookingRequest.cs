using System.ComponentModel.DataAnnotations;

public class CreateBookingRequest
{
    [Required]
    [MinLength(1)]
    public List<int> GuestIds { get; set; } = new();

    [Range(1, int.MaxValue)]
    public int MainGuestId { get; set; }

    [Range(1, int.MaxValue)]
    public int RoomId { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Range(1, int.MaxValue)]
    public int NumberGuests { get; set; }
}
