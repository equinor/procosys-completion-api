using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;


namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public class SourceTestObject
{
    public SourceTestObject(
        Guid testGuid,
        string? testString,
        DateTime? testDate,
        DateTime? testDate2,
        bool testBool,
        int? testInt,
        NestedObject nestedObject,
        Guid? woGuid,
        Guid? swcrGuid,
        Guid? personOID,
        Guid? documentGuid)
    {
        TestGuid = testGuid;
        TestString = testString;
        TestDate = testDate;
        TestDate2 = testDate2;
        TestBool = testBool;
        TestInt = testInt;
        NestedObject = nestedObject;
        WoGuid = woGuid;
        SwcrGuid = swcrGuid;
        PersonOID = personOID;
        DocumentGuid = documentGuid;
    }

    public Guid TestGuid { get; }
    public string? TestString { get; }
    public DateTime? TestDate { get; }
    public DateTime? TestDate2 { get; }
    public bool TestBool { get; }
    public int? TestInt { get; }
    public NestedObject NestedObject { get; }
    public Guid? WoGuid { get; }
    public Guid? SwcrGuid { get; }
    public Guid? PersonOID { get; }
    public Guid? DocumentGuid { get; }
}

public class NestedObject
{
    public NestedObject(Guid guid)
    {
        Guid = guid;
    }
    public Guid Guid { get; }
}

[TestClass]
public class SyncUpdateHandlerTests
{
    private IOracleDBExecutor _oracleDBExecutorMock;

    private readonly string _sourceObjectName = "TestObject";
    private readonly string _sourceObjectNameMissingPrimary = "TestObjNoPrimary";
    private readonly string _sourceObjectNameMissingProperty = "TestObjMissingProp";
    private readonly string _sourceObjectNameMissingConfig = "NotInConfiguration";
    private readonly string _sourceObjectNameWrongConversion = "TestObjWrongConv";

    private readonly Guid _testGuid = new Guid("805519D7-0DB6-44B7-BF99-A0818CEA778E");
    private readonly Guid _testGuid2 = new Guid("11111111-2222-3333-4444-555555555555");

    private readonly string _testString = "test";
    private readonly DateTime _testDate = new DateTime(2023, 11, 29, 10, 20, 30);
    private readonly DateTime _testDate2 = new DateTime(2023, 11, 30, 10, 20, 30);
    private readonly bool _testBool = true;
    private readonly int _testInt = 1234;
    private NestedObject _nestedObject;
    private readonly Guid _woGuid = new Guid("11111111-2222-3333-4444-555555555555");
    private readonly Guid _swcrGuid = new Guid("11111111-2222-3333-4444-555555555555");
    private readonly Guid _personOid = new Guid("11111111-2222-3333-4444-555555555555");
    private readonly Guid _documentGuid = new Guid("11111111-2222-3333-4444-555555555555");

    private SyncUpdateHandler _syncUpdateHandler;
    private SourceTestObject _sourceTestObject;

    private string _expectedSqlUpdateStatement =
        "update TestTargetTable set " +
        "TestString = 'test', " +
        "TestDateWithTime = to_date('29/11/2023 10:20:30', 'DD/MM/YYYY HH24:MI:SS'), " +
        "TestDate = to_date('30/11/2023', 'DD/MM/YYYY'), " +
        "TestBool = 'Y', " +
        "TestInt = 1234, " +
        "TestLibId = 123456789, " +
        "WoGuidLibId = 123456789, " +
        "SwcrLibId = 123456789, " +
        "PersonOid = 123456789, " +
        "DocumentId = 123456789 " +
        "where TestGuid = '805519D70DB644B7BF99A0818CEA778E'";

    private string _expectedSqlUpdateStatementNullValues =
        "update TestTargetTable set " +
        "TestString = null, " +
        "TestDateWithTime = null, " +
        "TestDate = null, " +
        "TestBool = 'N', " +
        "TestInt = null, " +
        "TestLibId = null, " +
        "WoGuidLibId = null, " +
        "SwcrLibId = null, " +
        "PersonOid = null, " +
        "DocumentId = null " +
        "where TestGuid = '805519D70DB644B7BF99A0818CEA778E'";

    [TestInitialize]
    public void Setup()
    {
        _oracleDBExecutorMock = Substitute.For<IOracleDBExecutor>();
        _syncUpdateHandler = new SyncUpdateHandler(_oracleDBExecutorMock);
        _nestedObject = new NestedObject(_testGuid2);
        _sourceTestObject = new SourceTestObject(_testGuid, _testString, _testDate, _testDate2, _testBool, _testInt, _nestedObject, _woGuid, _swcrGuid, _personOid, _documentGuid);
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldReturnSqlStatmeent_WhenInputIsCorrect()
    {
        _oracleDBExecutorMock.ExecuteDBQueryForValueLookupAsync(Arg.Any<string>(), default).Returns("123456789");
        var testObjectMappingConfig = new TestObjectMappingConfig();
        var actualSqlUpdateStatement = await _syncUpdateHandler.BuildSqlUpdateStatementAsync(testObjectMappingConfig, _sourceTestObject, default);
        Assert.AreEqual(_expectedSqlUpdateStatement, actualSqlUpdateStatement);
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldReturnSqlStatmeent_WhenSourceObjectHasNullValues()
    {
        _oracleDBExecutorMock.ExecuteDBQueryForValueLookupAsync(Arg.Any<string>(), default).Returns("123456789");

        _sourceTestObject = new SourceTestObject(_testGuid, null, null, null, false, null, null, null, null, null, null);

        var testObjectMappingConfig = new TestObjectMappingConfig();
        var actualSqlUpdateStatement = await _syncUpdateHandler.BuildSqlUpdateStatementAsync(testObjectMappingConfig, _sourceTestObject, default);
        Assert.AreEqual(_expectedSqlUpdateStatementNullValues, actualSqlUpdateStatement);
    }


    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldThrowException_WhenMissingProperty()
    {
        var testObjectMissingPropMappingConfig = new TestObjectMissingPropMappingConfig();

        var exception = await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _syncUpdateHandler.BuildSqlUpdateStatementAsync(testObjectMissingPropMappingConfig, _sourceTestObject, default);
        });

        Assert.AreEqual($"A property in configuration is missing in source object: PropMissing", exception.Message);
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldThrowException_WhenMissingConfigForObject()
    {
        var syncService = new SyncToPCS4Service(_oracleDBExecutorMock);

        var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await syncService.SyncUpdatesAsync(_sourceObjectNameMissingConfig, _sourceTestObject, default);
        });

        Assert.AreEqual($"Mapping is not implemented for source object with name '{_sourceObjectNameMissingConfig}'.", exception.Message);
    }
}
