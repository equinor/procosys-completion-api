using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public class PunchHelper : IPunchHelper
{
    private readonly IReadOnlyContext _context;

    public PunchHelper(IReadOnlyContext context) => _context = context;

    public async Task<Guid?> GetProjectGuidForPunchAsync(Guid punchGuid)
    {
        var project = await (from p in _context.QuerySet<Project>()
            join punch in _context.QuerySet<Punch>() on p.Id equals punch.ProjectId
            where punch.Guid == punchGuid
            select p).SingleOrDefaultAsync();

        return project?.Guid;
    }
}
