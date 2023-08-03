namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

// ReSharper disable InconsistentNaming
// Keep enum names to match types in ProCoSys 4
public enum LibraryType
{
    COMPLETION_ORGANIZATION,
    // In ProCoSys 4 PUNCHLIST_PRIORITY is COMM_PRIORITY classified as PUNCH_PRIORITY.
    // In ProCoSys 5 this should be simplefied to just PUNCHLIST_PRIORITY
    PUNCHLIST_PRIORITY,
    PUNCHLIST_SORTING,
    PUNCHLIST_TYPE
}
