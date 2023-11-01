namespace Equinor.ProCoSys.DbSyncPOC
{
    public class SyncEvent
    {
        public enum OperationType
        {
            Insert,
            Update,
            Delete
        }

        public required OperationType Operation { get; set; }
        public required string Table { get; set; }
        public required List<Column> Columns { get; set; }

        //Data: GUID: 12341234, Description: 'qwefq', etc

    }
}
