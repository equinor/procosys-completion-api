using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public class OracleGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();

        if (s is null)
        {
            throw new NullReferenceException();
        }

        return Guid.Parse(s);
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
