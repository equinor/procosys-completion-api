namespace Equinor.ProCoSys.Completion.WebApi.Misc;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CustomNullableDateTimeConverter : JsonConverter<DateTime?> 
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            return null;
        }

        return DateTime.Parse(dateString);
    }
    //Default implementation of Write method
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, typeof(DateTime?), options);
}
