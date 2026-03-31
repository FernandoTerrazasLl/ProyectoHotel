public class SimpleRoomTypePresetProduct : IRoomTypePresetProduct
{
    public string TypeName => "Simple";
    public int Capacity => 1;
    public string Description => "Habitación simple para una persona.";
    public decimal ReferencePrice => 120m;
}
