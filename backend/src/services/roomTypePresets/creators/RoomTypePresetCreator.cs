using System.Globalization;
using System.Text;

public abstract class RoomTypePresetCreator : IRoomTypePresetCreator
{
    public abstract bool CanHandle(string roomTypeName);
    public abstract IRoomTypePresetProduct CreateProduct();

    protected static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
