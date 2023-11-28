namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public class ColumnUpdate
    {
        public ColumnUpdate(string targetColumnName, string? targetColumnValue)
        {
            TargetColumnName = targetColumnName;
            TargetColumnValue = targetColumnValue;
        }

        public string TargetColumnName { get; }
        public string? TargetColumnValue { get; }
    }
}
