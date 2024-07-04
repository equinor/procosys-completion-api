namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

// ReSharper disable InconsistentNaming
// Keep enum names to match types in ProCoSys 4
public enum LibraryType
{
    COMPLETION_ORGANIZATION,
    // In ProCoSys 4 a Punch priority is a COMM_PRIORITY classified as PUNCH_PRIORITY.
    COMM_PRIORITY,
    PUNCHLIST_SORTING,
    PUNCHLIST_TYPE
}
