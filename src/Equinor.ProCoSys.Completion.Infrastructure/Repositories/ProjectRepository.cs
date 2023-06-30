using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class ProjectRepository : EntityWithGuidRepository<Project>, IProjectRepository
{
    public ProjectRepository(CompletionContext context)
        : base(context, context.Projects)
            
    {
    }
}
