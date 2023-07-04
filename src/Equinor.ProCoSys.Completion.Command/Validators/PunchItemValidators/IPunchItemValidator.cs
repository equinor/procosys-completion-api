using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;

public interface IPunchItemValidator
{
    Task<bool> ExistsAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> TagOwningPunchIsVoidedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    Task<bool> ProjectOwningPunchIsClosedAsync(Guid punchItemGuid, CancellationToken cancellationToken);
}
