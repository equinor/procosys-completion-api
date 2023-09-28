using System.Diagnostics.CodeAnalysis;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Category
{
    // DON'T CHANGE VALUES DUE TO STORED VALUES IN DB / SORTING
    PA = 0,
    PB = 1
}
