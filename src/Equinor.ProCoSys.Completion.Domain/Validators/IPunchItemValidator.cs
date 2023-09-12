using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface IPunchItemValidator
{
    Task<bool> ExistsAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> TagOwningPunchItemIsVoidedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> ProjectOwningPunchItemIsClosedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> IsClearedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> IsVerifiedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
}
