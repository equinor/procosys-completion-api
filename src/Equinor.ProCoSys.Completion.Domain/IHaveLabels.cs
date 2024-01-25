using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IHaveLabels
{
    IReadOnlyCollection<Label> Labels { get; }
    IOrderedEnumerable<Label> GetOrderedNonVoidedLabels();
    void UpdateLabels(IList<Label> labels);
}
