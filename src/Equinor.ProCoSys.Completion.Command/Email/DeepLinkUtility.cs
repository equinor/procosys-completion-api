using System;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Command.Email;

public class DeepLinkUtility : IDeepLinkUtility
{
    private readonly IOptionsMonitor<ApplicationOptions> _options;
    private readonly IPlantProvider _plantProvider;

    public DeepLinkUtility(IPlantProvider plantProvider, IOptionsMonitor<ApplicationOptions> options)
    {
        _plantProvider = plantProvider;
        _options = options;
    }

    public string CreateUrl(string contextName, Guid guid)
    {
        var plant = GetPlantNameWithoutPCSSuffix();
        return contextName switch
        {
            // todo 109830 Deep link to the punch item
            nameof(PunchItem) => $"{_options.CurrentValue.BaseUrl.TrimEnd('/')}/{plant}",
            _ => throw new NotImplementedException($"DeepLinkUtility.CreateUrl not implemented for {contextName}")
        };
    }

    private string GetPlantNameWithoutPCSSuffix() => _plantProvider.Plant[4..];
}
