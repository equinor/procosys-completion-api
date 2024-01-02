namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public class TargetColumnData
{
    public TargetColumnData(string columnName, object? columnValue, bool nextValGeneration = false)
    {
        ColumnName = columnName;
        ColumnValue = columnValue;
        NextValGeneration = nextValGeneration;
    }

    public string ColumnName { get; }
    public object? ColumnValue { get; }
    public bool NextValGeneration { get; }
}

