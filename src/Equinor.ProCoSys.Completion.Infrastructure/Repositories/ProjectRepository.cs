using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class ProjectRepository : EntityWithGuidRepository<Project>, IProjectRepository
{
    public ProjectRepository(CompletionContext context)
        : base(context, context.Projects, context.Projects)
            
    {
    }

    public Task<Project?> TryGetProjectByNameAsync(string projectName)
        => DefaultQuery.SingleOrDefaultAsync(p => !string.IsNullOrEmpty(projectName) &&  p.Name == projectName);
}
