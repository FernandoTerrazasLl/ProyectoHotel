public class DoubleMatrimonialRoomTypePresetCreator : RoomTypePresetCreator
{
    public override bool CanHandle(string roomTypeName)
    {
        var normalized = Normalize(roomTypeName);
        return normalized == "doble matrimonial";
    }

    public override IRoomTypePresetProduct CreateProduct()
    {
        return new DoubleMatrimonialRoomTypePresetProduct();
    }
}