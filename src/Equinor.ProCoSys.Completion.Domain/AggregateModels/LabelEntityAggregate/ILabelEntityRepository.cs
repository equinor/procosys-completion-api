using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;

public interface ILabelEntityRepository : IRepository<LabelEntity>
{
    Task<bool> ExistsAsync(EntityWithLabelType entityWithLabelType, CancellationToken cancellationToken);
}
