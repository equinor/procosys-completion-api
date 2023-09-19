using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public class PunchItemHelper : IPunchItemHelper
{
    private readonly IReadOnlyContext _context;

    public PunchItemHelper(IReadOnlyContext context) => _context = context;

    public async Task<Guid?> GetProjectGuidForPunchItemAsync(Guid punchItemGuid)
    {
        var project = await (from p in _context.QuerySet<Project>()
            join punchItem in _context.QuerySet<PunchItem>() on p.Id equals punchItem.ProjectId
            where punchItem.Guid == punchItemGuid
            select p).SingleOrDefaultAsync();

        return project?.Guid;
    }

    public async Task<Guid?> GetCheckListGuidForPunchItemAsync(Guid punchItemGuid)
    {
        var punchItem = await (from pi in _context.QuerySet<PunchItem>()
            where pi.Guid == punchItemGuid
            select pi).SingleOrDefaultAsync();

        return punchItem?.CheckListGuid;

    }
}
