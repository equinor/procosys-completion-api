namespace Equinor.ProCoSys.Completion.WebApi.Misc;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            throw new JsonException("The date string is null or empty.");
        }
        
        if(DateTime.TryParse(dateString, out var dateTime))
        {
            return dateTime;
        }
        else
        {
            return DateTime.ParseExact(dateString, DateFormat, CultureInfo.InvariantCulture);
        }
       
    }
    //Default implementation of Write method
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, typeof(DateTime), options);
}
