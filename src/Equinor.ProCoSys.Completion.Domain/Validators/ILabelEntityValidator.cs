using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface ILabelEntityValidator
{
    Task<bool> ExistsAsync(EntityTypeWithLabels entityType, CancellationToken cancellationToken);
}
