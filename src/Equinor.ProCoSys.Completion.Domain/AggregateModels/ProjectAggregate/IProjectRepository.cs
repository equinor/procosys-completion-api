using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;

public interface IProjectRepository : IRepositoryWithGuid<Project>
{
    Task<Project?> TryGetProjectByNameAsync(string projectName);
}
