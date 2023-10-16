using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface IWorkOrderValidator
{
    Task<bool> ExistsAsync(Guid workOrderGuid, CancellationToken cancellationToken);
    Task<bool> IsClosedAsync(Guid workOrderGuid, CancellationToken cancellationToken);
}
