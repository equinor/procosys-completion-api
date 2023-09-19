using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.WebApi.Validators.ProjectValidators;

public class ProjectValidator : IProjectValidator
{
    private readonly IReadOnlyContext _context;

    public ProjectValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid projectGuid, CancellationToken cancellationToken) =>
        await (from p in _context.QuerySet<Project>()
            where p.Guid == projectGuid
            select p).AnyAsync(cancellationToken);

    public async Task<bool> IsClosedAsync(Guid projectGuid, CancellationToken cancellationToken)
    {
        var project = await (from p in _context.QuerySet<Project>()
            where p.Guid == projectGuid
            select p).SingleOrDefaultAsync(cancellationToken);

        return project is not null && project.IsClosed;
    }
}
