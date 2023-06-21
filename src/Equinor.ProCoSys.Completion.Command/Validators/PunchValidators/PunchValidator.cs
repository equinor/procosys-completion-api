using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Common;
using System;

namespace Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;

public class PunchValidator : IPunchValidator
{
    private readonly IReadOnlyContext _context;

    public PunchValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> PunchExistsAsync(Guid punchGuid, CancellationToken cancellationToken) =>
        await (from punch in _context.QuerySet<Punch>()
            where punch.Guid == punchGuid
            select punch).AnyAsync(cancellationToken);

    public Task<bool> TagOwingPunchIsVoidedAsync(Guid punchGuid, CancellationToken cancellationToken)
    {
        // todo #103935 update code below to query tag table to check if tag for punch is voided 
        //var tag = await (from punch in _context.QuerySet<Punch>()
        //    join t in _context.QuerySet<Tag>() on punch.TagId equals t.Id 
        //    where punch.Guid == punchGuid
        //    select t).SingleOrDefaultAsync(cancellationToken);
        //return tag != null && tag.IsVoided;
        // ReSharper disable once ArrangeMethodOrOperatorBody
        return Task.FromResult(false);
    }

    public async Task<bool> ProjectOwningPunchIsClosedAsync(Guid punchGuid, CancellationToken cancellationToken)
    {
        var project = await (from punch in _context.QuerySet<Punch>()
            join p in _context.QuerySet<Project>() on punch.ProjectId equals p.Id
            where punch.Guid == punchGuid
            select p).SingleOrDefaultAsync(cancellationToken);

        return project != null && project.IsClosed;
    }
}
