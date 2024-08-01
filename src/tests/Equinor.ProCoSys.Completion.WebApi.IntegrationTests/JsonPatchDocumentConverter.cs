using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public sealed class JsonPatchDocumentConverter : JsonConverter<JsonPatchDocument>
{
    public override JsonPatchDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialization logic, if needed
        return JsonSerializer.Deserialize<JsonPatchDocument>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, JsonPatchDocument value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Operations, options);
    }
}
