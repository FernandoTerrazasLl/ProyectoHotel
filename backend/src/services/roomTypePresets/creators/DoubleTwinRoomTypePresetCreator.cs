public class DoubleTwinRoomTypePresetCreator : RoomTypePresetCreator
{
    public override bool CanHandle(string roomTypeName)
    {
        var normalized = Normalize(roomTypeName);
        return normalized == "doble con camas individuales" || normalized == "doble twin";
    }

    public override IRoomTypePresetProduct CreateProduct()
    {
        return new DoubleTwinRoomTypePresetProduct();
    }
}