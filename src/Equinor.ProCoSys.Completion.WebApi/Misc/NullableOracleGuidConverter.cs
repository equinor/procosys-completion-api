using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public class NullableOracleGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      
        var s = reader.GetString();

        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        return Guid.Parse(s);
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString());
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
