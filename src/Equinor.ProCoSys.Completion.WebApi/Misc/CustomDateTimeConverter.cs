namespace Equinor.ProCoSys.Completion.WebApi.Misc;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            throw new JsonException("The date string is null or empty.");
        }

        return DateTime.Parse(dateString);
    }
    //Default implementation of Write method
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, typeof(DateTime), options);
}
