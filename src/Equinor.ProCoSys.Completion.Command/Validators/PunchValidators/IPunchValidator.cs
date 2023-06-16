using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;

public interface IPunchValidator
{
    Task<bool> PunchExistsAsync(Guid punchGuid, CancellationToken cancellationToken);
    Task<bool> PunchIsVoidedAsync(Guid punchGuid, CancellationToken cancellationToken);
}
