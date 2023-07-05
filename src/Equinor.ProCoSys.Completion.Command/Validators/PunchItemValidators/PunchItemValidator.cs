using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Common;
using System;

namespace Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;

public class PunchItemValidator : IPunchItemValidator
{
    private readonly IReadOnlyContext _context;

    public PunchItemValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid punchItemGuid, CancellationToken cancellationToken) =>
        await (from punchItem in _context.QuerySet<PunchItem>()
            where punchItem.Guid == punchItemGuid
            select punchItem).AnyAsync(cancellationToken);

    public Task<bool> TagOwningPunchItemIsVoidedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        // todo #103935 update code below to query tag table to check if tag for punch item is voided .. remember unit test
        //var tag = await (from punchItem in _context.QuerySet<PunchItem>()
        //    join t in _context.QuerySet<Tag>() on punchItem.TagId equals t.Id 
        //    where punchItem.Guid == punchItemGuid
        //    select t).SingleOrDefaultAsync(cancellationToken);
        //return tag is not null && tag.IsVoided;
        return Task.FromResult(false);
    }

    public async Task<bool> ProjectOwningPunchItemIsClosedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var project = await (from punchItem in _context.QuerySet<PunchItem>()
            join proj in _context.QuerySet<Project>() on punchItem.ProjectId equals proj.Id
            where punchItem.Guid == punchItemGuid
            select proj).SingleOrDefaultAsync(cancellationToken);

        return project is not null && project.IsClosed;
    }

    public async Task<bool> IsReadyToBeClearedAsync(Guid punchGuid, CancellationToken cancellationToken)
    {
        var punchItem = await (from p in _context.QuerySet<PunchItem>()
            where p.Guid == punchGuid
            select p).SingleOrDefaultAsync(cancellationToken);

        return punchItem != null && punchItem.IsReadyToBeCleared;
    }
}
