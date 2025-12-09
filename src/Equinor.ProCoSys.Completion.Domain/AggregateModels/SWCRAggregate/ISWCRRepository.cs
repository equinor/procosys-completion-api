using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;

public interface ISWCRRepository : IRepositoryWithGuid<SWCR>
{
    Task<SWCR?> GetByNoAsync(int swcrNo, CancellationToken cancellationToken);
}
