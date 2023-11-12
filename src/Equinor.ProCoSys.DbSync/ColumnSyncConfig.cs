using static Equinor.ProCoSys.DbSyncPOC.Column;

namespace Equinor.ProCoSys.DbSyncPOC
{
    internal class ColumnSyncConfig
    {
        public required string SourceTable { get; set; }
        public required string TargetTable { get; set; }
        public required string SourceColumn { get; set; }
        public required string TargetColumn { get; set; }
        public required bool IsPrimaryKey { get; set; } = false;
        public required DataType SourceType { get; set; }
        public string? ValueConvertionMethod { get; set; }
    }
}
