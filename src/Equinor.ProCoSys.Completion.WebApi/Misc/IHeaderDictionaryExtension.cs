using Microsoft.AspNetCore.Http;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public static class IHeaderDictionaryExtension
{
    public const string PlantHeader = "x-plant";

    public static string? GetPlant(this IHeaderDictionary headers)
    {
        if (headers.TryGetValue(PlantHeader, out var header))
        {
            return header.ToString().ToUpperInvariant();
        }

        return null;
    }
}
