using System.Text.Json.Serialization;

public class RoomType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }

    [JsonIgnore]
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
