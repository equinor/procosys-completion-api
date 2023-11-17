using System.Configuration;
using Equinor.ProCoSys.Completion.DbSyncToPOCS4;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{

    public class PunchItem
    {
        public Guid? Guid { get; set; }
        public string? Description { get; set; }
        public string? RaisedByOrg { get; set; }

    }


    public class Test
    {

        public static void Main()
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<Test>();

            IConfigurationRoot configuration = builder.Build();

            string dbConn = configuration["OracleDBConnectionPOC"];

            DbSynchronizer.SetOracleConnection(dbConn);
            var test = new Test();
            test.UpdatePunchTest();
        }

        public void UpdatePunchTest()
        {
            var punchItem = new PunchItem { Guid = new Guid("eb38cccb-4917-d926-e053-2810000ac5b2"), Description = "sdf" };
            DbSynchronizer.SyncChangesToMain(punchItem);
        }
    }
}

