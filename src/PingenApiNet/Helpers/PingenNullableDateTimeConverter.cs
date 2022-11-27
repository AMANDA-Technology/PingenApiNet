using System.Text.Json;
using System.Text.Json.Serialization;

namespace PingenApiNet.Helpers;

/// <summary>
///
/// </summary>
internal class PingenNullableDateTimeConverter : JsonConverter<DateTime?>
{
    /// <summary>
    ///
    /// </summary>
    private const string PingenDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valueString = reader.GetString();

        if (string.IsNullOrEmpty(valueString)) return null;
        return DateTime.TryParseExact(valueString, PingenDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var value) ? value : null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(PingenDateTimeFormat));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
