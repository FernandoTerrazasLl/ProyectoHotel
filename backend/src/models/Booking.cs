using System;
using System.Collections.Generic;

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberGuests { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal? CancellationFee { get; set; }
    public DateTime CreatedAt { get; set; }

    public Room? Room { get; set; }
    public ICollection<GuestBooking> GuestBookings { get; set; } = new List<GuestBooking>();
}
