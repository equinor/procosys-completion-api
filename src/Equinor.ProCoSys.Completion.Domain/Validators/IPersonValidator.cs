using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface IPersonValidator
{
    Task<bool> ExistsAsync(Guid oid, CancellationToken cancellationToken);
}
