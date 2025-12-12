using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;

public interface IDocumentRepository : IRepositoryWithGuid<Document>
{

    Task<Document?> GetByDocNoAsync(string documentNo, CancellationToken cancellationToken);
}
