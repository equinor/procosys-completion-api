using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;

public interface ILabelEntityRepository : IRepository<LabelEntity>
{
    Task<LabelEntity> GetByTypeAsync(EntityTypeWithLabels entityType, CancellationToken cancellationToken);
}
