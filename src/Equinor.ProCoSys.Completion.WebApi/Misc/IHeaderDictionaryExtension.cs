using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Microsoft.AspNetCore.Http;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public static class IHeaderDictionaryExtension
{
    public static string? GetPlant(this IHeaderDictionary headers)
    {
        if (headers.TryGetValue(CurrentPlantMiddleware.PlantHeader, out var header))
        {
            return header.ToString().ToUpperInvariant();
        }

        return null;
    }
}
