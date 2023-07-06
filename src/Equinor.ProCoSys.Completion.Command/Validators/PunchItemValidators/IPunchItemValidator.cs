using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;

public interface IPunchItemValidator
{
    Task<bool> ExistsAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> TagOwningPunchItemIsVoidedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> ProjectOwningPunchItemIsClosedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> IsClearedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
}
