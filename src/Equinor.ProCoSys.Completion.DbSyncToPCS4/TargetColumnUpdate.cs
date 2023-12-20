namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public class TargetColumnUpdate
{
    public TargetColumnUpdate(string columnName, object? columnValue)
    {
        ColumnName = columnName;
        ColumnValue = columnValue;
    }

    public string ColumnName { get; }
    public object? ColumnValue { get; }
}

