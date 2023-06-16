using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
}
