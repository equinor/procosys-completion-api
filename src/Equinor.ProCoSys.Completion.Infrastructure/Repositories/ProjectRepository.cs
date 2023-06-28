using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class ProjectRepository : EntityWithGuidRepository<Project>, IProjectRepository
{
    public ProjectRepository(CompletionContext context)
        : base(context, context.Projects)
            
    {
    }
}
