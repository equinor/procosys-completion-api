using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

public interface ILabelRepository : IRepository<Label>
{
    Task<List<Label>> GetManyAsync(IEnumerable<string> texts, CancellationToken cancellationToken);
    Task<Label> GetByTextAsync(string text, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string text, CancellationToken cancellationToken);
}
