using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public static class IConfigurationExtension
{
    public static bool IsDevOnLocalhost(this IConfiguration configuration)
        => configuration.GetValue<bool>("Application:DevOnLocalhost");
}
