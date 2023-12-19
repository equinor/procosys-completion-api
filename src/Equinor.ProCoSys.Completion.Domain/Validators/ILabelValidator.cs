using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface ILabelValidator
{
    Task<bool> ExistsAsync(string label, CancellationToken cancellationToken);
    Task<bool> IsVoidedAsync(string label, CancellationToken cancellationToken);
}
