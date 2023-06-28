using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators
{
    public interface IProjectValidator
    {
        Task<bool> ExistsAsync(Guid projectGuid, CancellationToken cancellationToken);
        Task<bool> IsClosedAsync(Guid projectGuid, CancellationToken cancellationToken);
    }
}
