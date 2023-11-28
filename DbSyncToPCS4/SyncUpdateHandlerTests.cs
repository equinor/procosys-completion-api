using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using NSubstitute;

namespace DbSyncToPCS4
{
    public class TestObject
    {
        public TestObject(Guid testGuid, string testString, DateTime testDate, bool testBool, int testInt, Guid testDocument)
        {
            TestGuid = testGuid;
            TestString = testString;
            TestDate = testDate;
            TestBool = testBool;
            TestInt = testInt;
            TestDocument = testDocument;
        }

        public Guid TestGuid { get; set; }
        public string TestString { get; set; }
        public DateTime TestDate { get; set; }
        public bool TestBool { get; set; }
        public int TestInt { get; set; }
        public Guid TestDocument { get; set; }
    }

    [TestClass]
    public class SyncUpdateHandlerTests
    {
        private IOracleDBExecutor _oracleDBExecutorMock;
        private CancellationToken _cancellationToken = new();
        private string _sourceObjectName = "TestObject";
        private object _sourceObject;
        private SyncMappingConfig _syncMappingConfig;

        [TestInitialize]
        public void Setup()
        {
            _oracleDBExecutorMock = Substitute.For<IOracleDBExecutor>();
            // _oracleDBExecutorMock.ExecuteDBQueryForValueLookupAsync("test", _cancellationToken).Returns(Task.FromResult("asdf"));

        }

        [TestMethod]
        public async Task BuildSqlUpdateStatement_With_Correct_Input()
        {
            var syncUpdateHandler = new SyncUpdateHandler(_oracleDBExecutorMock);

            var sqlUpdateStatement = await syncUpdateHandler.BuildSqlUpdateStatementAsync(_sourceObjectName, _sourceObject, _syncMappingConfig, _cancellationToken);
        }

        [TestMethod]
        public async Task BuildSqlUpdateStatement_With_MissingPropertyInSourceObject()
        {

        }
        [TestMethod]
        public async Task BuildSqlUpdateStatement_With_MissingSourceObjectConfig()
        {
        }
        [TestMethod]
        public async Task BuildSqlUpdateStatement_With_ConvertionMethodNotExists()
        {
        }
    }
}