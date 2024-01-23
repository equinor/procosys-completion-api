using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface ISWCRValidator
{
    Task<bool> ExistsAsync(Guid swcrGuid, CancellationToken cancellationToken);
    Task<bool> IsVoidedAsync(Guid swcrGuid, CancellationToken cancellationToken);
}
