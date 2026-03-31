public class SuiteRoomTypePresetCreator : RoomTypePresetCreator
{
    public override bool CanHandle(string roomTypeName)
    {
        return Normalize(roomTypeName) == "suite";
    }

    public override IRoomTypePresetProduct CreateProduct()
    {
        return new SuiteRoomTypePresetProduct();
    }
}