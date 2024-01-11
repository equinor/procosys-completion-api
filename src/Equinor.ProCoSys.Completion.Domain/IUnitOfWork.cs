using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IUnitOfWork
{
    Task SetAuditDataAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
