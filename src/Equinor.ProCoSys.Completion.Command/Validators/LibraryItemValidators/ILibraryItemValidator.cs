using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.Command.Validators.LibraryItemValidators;

public interface ILibraryItemValidator
{
    Task<bool> ExistsAsync(Guid libraryItemGuid, CancellationToken cancellationToken);
    Task<bool> HasTypeAsync(Guid libraryItemGuid, LibraryTypes type, CancellationToken cancellationToken);
    Task<bool> IsVoidedAsync(Guid libraryItemGuid, CancellationToken cancellationToken);
}
