using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

public interface ILibraryItemRepository : IRepositoryWithGuid<LibraryItem>
{
    Task<LibraryItem> GetByGuidAndTypeAsync(Guid libraryGuid, LibraryType type, CancellationToken cancellationToken);
}
