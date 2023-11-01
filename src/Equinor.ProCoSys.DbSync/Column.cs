namespace Equinor.ProCoSys.DbSyncPOC
{
    public class Column
    {

        public enum DataType
        {
            String,
            Int,
            Date
        }

        public DataType Type { get; set; } = DataType.String;
        public required string Name { get; set; }
        public required string? Value { get; set; }

    }
}
