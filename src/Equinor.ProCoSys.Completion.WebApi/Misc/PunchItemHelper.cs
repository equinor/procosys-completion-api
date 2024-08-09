using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public class PunchItemHelper(IReadOnlyContext context) : IPunchItemHelper
{
    public async Task<Guid?> GetProjectGuidForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var projectGuid = await (from p in context.QuerySet<Project>()
                .TagWith($"{nameof(PunchItemHelper)}.{nameof(GetProjectGuidForPunchItemAsync)}")
            join punchItem in context.QuerySet<PunchItem>() on p.Id equals punchItem.ProjectId
            where punchItem.Guid == punchItemGuid
            select p.Guid).SingleOrDefaultAsync(cancellationToken);

        return projectGuid == default ? null : projectGuid;
    }

    public async Task<Guid?> GetCheckListGuidForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var punchItemCheckListGuid = await (from pi in context.QuerySet<PunchItem>()
                .TagWith($"{nameof(PunchItemHelper)}.{nameof(GetCheckListGuidForPunchItemAsync)}")
            where pi.Guid == punchItemGuid
            select pi.CheckListGuid).SingleOrDefaultAsync(cancellationToken);

        return punchItemCheckListGuid == default ? null : punchItemCheckListGuid;
    }
}
