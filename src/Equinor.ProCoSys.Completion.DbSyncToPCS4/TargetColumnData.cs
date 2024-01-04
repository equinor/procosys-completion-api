namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public class TargetColumnData(string columnName, object? columnValue, bool nextValGeneration = false)
{
    public string ColumnName { get; } = columnName;
    public object? ColumnValue { get; } = columnValue;
    public bool NextValGeneration { get; } = nextValGeneration;
}

