namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public class ColumnUpdate
    {
        public required string TargetColumnName { get; set; }
        public required string? TargetValue { get; set; }
    }
}
