using static Equinor.ProCoSys.Completion.DbSyncToPCS4.SyncMappingConfig;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public class ColumnSyncConfig
    {
        public required string SourceObjectName { get; set; }
        public required string TargetTable { get; set; }
        public required string SourceProperty { get; set; }
        public required string TargetColumn { get; set; }
        public required bool IsPrimaryKey { get; set; } = false;
        public required DataColumnType SourceType { get; set; } 
        public string? ValueConvertionMethod { get; set; }
    }
}
