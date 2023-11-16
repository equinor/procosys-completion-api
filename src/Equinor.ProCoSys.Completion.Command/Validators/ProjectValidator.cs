using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class ProjectValidator : IProjectValidator
{
    private readonly IReadOnlyContext _context;

    // Trick to write LINQ queries to let EF create effective SQL queries is
    // 1) use Any
    // 2) select a projection with as few columns as needed
    public ProjectValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid projectGuid, CancellationToken cancellationToken) =>
        await (from p in _context.QuerySet<Project>()
            where p.Guid == projectGuid
            select 1).AnyAsync(cancellationToken);

    public async Task<bool> IsClosedAsync(Guid projectGuid, CancellationToken cancellationToken)
        => await (from p in _context.QuerySet<Project>()
            where p.Guid == projectGuid && p.IsClosed == true
            select 1).AnyAsync(cancellationToken);
}
