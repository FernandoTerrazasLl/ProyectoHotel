using System.Text.Json.Serialization;

public class Room
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public int Floor { get; set; }
    public bool IsActive { get; set; } = true;

    public RoomType? RoomType { get; set; }
    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
