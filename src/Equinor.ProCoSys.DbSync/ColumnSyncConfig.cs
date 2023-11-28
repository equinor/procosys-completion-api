using static Equinor.ProCoSys.Completion.DbSyncToPCS4.SyncMappingConfig;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public class ColumnSyncConfig
    {
        public ColumnSyncConfig(
            string sourceObjectName,
            string sourceProperty,
            string targetTable,
            string targetColumn,
            bool isPrimaryKey,
            DataColumnType sourceType,
            string? valueConversionMethod)
        {
            SourceObjectName = sourceObjectName;
            SourceProperty = sourceProperty;
            TargetTable = targetTable;
            TargetColumn = targetColumn;
            IsPrimaryKey = isPrimaryKey;
            SourceType = sourceType;
            ValueConversionMethod = valueConversionMethod;
        }

        public string SourceObjectName { get; }
        public string SourceProperty { get; }
        public string TargetTable { get; }
        public string TargetColumn { get; }
        public bool IsPrimaryKey { get; }
        public DataColumnType SourceType { get; }
        public string? ValueConversionMethod { get; }
    }
}
