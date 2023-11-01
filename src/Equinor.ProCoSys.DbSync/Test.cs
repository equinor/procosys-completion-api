namespace Equinor.ProCoSys.DbSyncPOC
{
    public class Test
    {

        public static void Main()
        {
            var test = new Test();
            test.UpdatePunchTest();
        }

        public void UpdatePunchTest()
        {
            SyncEvent updatePunch = new SyncEvent
            {
                Operation = SyncEvent.OperationType.Update,
                Table = "punchitem",
                Columns = new List<Column> {
                   new Column { Type=Column.DataType.String, Name="guid", Value="EB38CCCB492AD926E0532810000AC5B2"},
                   new Column { Name="description", Value="7003 - LOW DFT AS PER ATTACHED DRAWING MARKT 3(PCSwin: 2151 - X001 / Item 3)"},
                }
            };

            var dbSync = new DbSynchronizer();

            dbSync.HandleEvent(updatePunch);
        }
    }
}

