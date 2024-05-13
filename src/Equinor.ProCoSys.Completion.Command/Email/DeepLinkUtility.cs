using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Command.Email;

public class DeepLinkUtility(IPlantProvider plantProvider, IOptionsMonitor<ApplicationOptions> options)
    : IDeepLinkUtility
{
    private readonly Dictionary<string, Func<DeepLinkCreatorArgs, string>> _deepLinkCreators = new()
    {
        { nameof(PunchItem), PunchItemDeepLinkCreator }
    };

    public string CreateUrl(string contextName, Guid guid)
    {
        var plant = GetPlantNameWithoutPCSPrefix();
        var args = new DeepLinkCreatorArgs(plant, guid, options);

        if (!_deepLinkCreators.TryGetValue(contextName, out var creator))
        {
            throw new NotImplementedException($"DeepLinkUtility.CreateUrl not implemented for {contextName}");
        }

        return creator(args);
    }

    private string GetPlantNameWithoutPCSPrefix() => plantProvider.Plant[4..];

    private static string PunchItemDeepLinkCreator(DeepLinkCreatorArgs args) =>
        $"{args.Options.CurrentValue.BaseUrl.TrimEnd('/')}/{args.Plant}/Completion/PunchListItem/RedirectToPunchListItemView?punchListItemGuid={args.Guid}";

    private sealed record DeepLinkCreatorArgs(string Plant, Guid Guid, IOptionsMonitor<ApplicationOptions> Options);
}
