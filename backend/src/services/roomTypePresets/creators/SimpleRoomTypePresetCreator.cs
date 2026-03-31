public class SimpleRoomTypePresetCreator : RoomTypePresetCreator
{
    public override bool CanHandle(string roomTypeName)
    {
        return Normalize(roomTypeName) == "simple";
    }

    public override IRoomTypePresetProduct CreateProduct()
    {
        return new SimpleRoomTypePresetProduct();
    }
}