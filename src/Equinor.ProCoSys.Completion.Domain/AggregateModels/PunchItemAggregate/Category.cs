using System.Diagnostics.CodeAnalysis;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

// Punch Category in PCS5 corresponds to LibraryType=COMPLETION_STATUS with Classification = PUNCHLIST in PCS4
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Category
{
    // DON'T CHANGE VALUES DUE TO STORED VALUES IN DB / SORTING
    PA = 0,
    PB = 1
}
