namespace Equinor.ProCoSys.DbSyncPOC
{
    public class Test
    {

        public static void Main()
        {

            var Value = "asdasfqwef-qwefqwef-qwef124";
            var test2 = Value.Replace("-", string.Empty);

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
                   new Column { Name="description", Value="7003 - LOW DFT AS PER ATTACHED DRAWING MARKT 3(PCSwin: 2151 - X001 / Item 3)"}
                },
                PrimaryKey = new Column { Type = Column.DataType.Guid, Name = "guid", Value = "eb38cccb-4917-d926-e053-2810000ac5b2" }
            };

            var dbSync = new DbSynchronizer();

            dbSync.HandleEvent(updatePunch);
        }
    }
}

