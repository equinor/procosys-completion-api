using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;

public interface IPunchValidator
{
    Task<bool> ExistsAsync(Guid punchGuid, CancellationToken cancellationToken);
    Task<bool> TagOwingPunchIsVoidedAsync(Guid punchGuid, CancellationToken cancellationToken);
    Task<bool> ProjectOwningPunchIsClosedAsync(Guid punchGuid, CancellationToken cancellationToken);
}
