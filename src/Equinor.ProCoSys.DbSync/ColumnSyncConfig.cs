namespace Equinor.ProCoSys.DbSyncPOC
{
    internal class ColumnSyncConfig
    {
        public required string SourceTable { get; set; }
        public required string TargetTable { get; set; }
        public required string SourceColumn { get; set; }
        public required string TargetColumn { get; set; }
        public bool IsPrimaryKey { get; set; } = false;
        public IValueConvertion? ValueConvertion { get; set; }


    }
}
