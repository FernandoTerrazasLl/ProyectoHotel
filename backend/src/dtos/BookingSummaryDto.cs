using System.Text.Json.Serialization;

public class BookingSummaryDto
{
    public int Id { get; set; }
    public int? GuestId { get; set; }
    public string GuestFullName { get; set; } = string.Empty;
    public int? MainGuestId { get; set; }
    public string MainGuestFullName { get; set; } = string.Empty;
    public List<BookingGuestDto> Guests { get; set; } = new();
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public string RoomTypeDescription { get; set; } = string.Empty;
    public int RoomTypeCapacity { get; set; }
    public decimal RoomTypePricePerNight { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberGuests { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingStatus Status { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal? CancellationFee { get; set; }
}
