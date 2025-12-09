using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;

public interface IDocumentRepository : IRepositoryWithGuid<Document>
{

    Task<Document?> GetByNoAsync(string documentNo, CancellationToken cancellationToken);
}
