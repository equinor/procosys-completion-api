using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators
{
    public class ProjectValidator : IProjectValidator
    {
        private readonly IReadOnlyContext _context;

        public ProjectValidator(IReadOnlyContext context) => _context = context;

        public async Task<bool> ExistsAsync(string projectName, CancellationToken cancellationToken) =>
            await (from p in _context.QuerySet<Project>()
                where p.Name == projectName
                select p).AnyAsync(cancellationToken);

        public async Task<bool> IsClosed(string projectName, CancellationToken cancellationToken)
        {
            var project = await (from p in _context.QuerySet<Project>()
                where p.Name == projectName
                select p).SingleOrDefaultAsync(cancellationToken);

            return project != null && project.IsClosed;
        }

        public async Task<bool> IsClosedForPunch(Guid punchGuid, CancellationToken cancellationToken)
        {
            var project = await (from punch in _context.QuerySet<Punch>()
                join p in _context.QuerySet<Project>() on punch.ProjectId equals p.Id
                where punch.Guid == punchGuid
                select p).SingleOrDefaultAsync(cancellationToken);

            return project != null && project.IsClosed;
        }
    }
}
