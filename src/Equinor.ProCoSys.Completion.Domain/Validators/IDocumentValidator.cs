using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface IDocumentValidator
{
    Task<bool> ExistsAsync(Guid documentGuid, CancellationToken cancellationToken);
    Task<bool> IsVoidedAsync(Guid documentGuid, CancellationToken cancellationToken);
}
