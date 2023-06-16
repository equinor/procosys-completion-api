using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators
{
    public interface IProjectValidator
    {
        Task<bool> ExistsAsync(string projectName, CancellationToken cancellationToken);
        Task<bool> IsClosed(string projectName, CancellationToken cancellationToken);
        Task<bool> IsClosedForPunch(Guid punchGuid, CancellationToken cancellationToken);
    }
}
