namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public class TargetColumnUpdate
{
    public TargetColumnUpdate(string columnName, string? columnValue)
    {
        ColumnName = columnName;
        ColumnValue = columnValue;
    }

    public string ColumnName { get; }
    public string? ColumnValue { get; }
}

