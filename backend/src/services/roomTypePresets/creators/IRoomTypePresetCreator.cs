public interface IRoomTypePresetCreator
{
    bool CanHandle(string roomTypeName);
    IRoomTypePresetProduct CreateProduct();
}