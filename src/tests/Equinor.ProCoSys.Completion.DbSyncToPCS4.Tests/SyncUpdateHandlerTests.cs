using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class SyncUpdateHandlerTests
{
    private IPcs4Repository _oracleDBExecutorMock;

    private readonly string _sourceObjectNameMissingConfig = "NotInConfiguration";

    private readonly Guid _testGuid = new Guid("805519D7-0DB6-44B7-BF99-A0818CEA778E");
    private readonly Guid _testGuid2 = new Guid("11111111-2222-3333-4444-555555555555");

    private readonly string _testString = "test";
    private readonly DateTime _testDate = new DateTime(2023, 11, 29, 10, 20, 30);
    private readonly DateTime _testDate2 = new DateTime(2023, 11, 30, 10, 20, 30);
    private readonly bool _testBool = true;
    private readonly int _testInt = 1234;
    private NestedSourceTestObject _nestedObject;
    private readonly Guid _woGuid = new Guid("11111111-2222-3333-4444-555555555555");
    private readonly Guid _swcrGuid = new Guid("11111111-2222-3333-4444-555555555555");
    private readonly Guid _personOid = new Guid("11111111-2222-3333-4444-555555555555");
    private readonly Guid _documentGuid = new Guid("11111111-2222-3333-4444-555555555555");

    private SqlUpdateStatementBuilder _syncUpdateHandler;
    private SourceTestObject _sourceTestObject;

    private string _expectedSqlUpdateStatement =
        "update TestTargetTable set " +
        "TestString = :TestString, " +
        "TestDateWithTime = :TestDateWithTime, " +
        "TestDate = :TestDate, " +
        "TestBool = :TestBool, " +
        "TestInt = :TestInt, " +
        "TestLibId = :TestLibId, " +
        "WoGuidLibId = :WoGuidLibId, " +
        "SwcrLibId = :SwcrLibId, " +
        "PersonOid = :PersonOid, " +
        "DocumentId = :DocumentId " +
        "where TestGuid = :TestGuid";


    private readonly Dictionary<string, object> _expectedSqlParameters = new()
    {
        { "TestString", "test" },
        { "TestDateWithTime", new DateTime(2023, 11, 29, 10, 20, 30)},
        { "TestDate", new DateTime(2023, 11, 30, 10, 20, 30) },
        { "TestBool", "Y" },
        { "TestInt" , 1234 },
        { "TestLibId", 123456789 } ,
        { "WoGuidLibId" , 123456789 },
        { "SwcrLibId" , 123456789 } ,
        { "PersonOid" , 123456789 },
        { "DocumentId" , 123456789 },
        { "TestGuid" , "805519D70DB644B7BF99A0818CEA778E" }
    };

    private readonly Dictionary<string, string> _expectedSqlParametersNullValues = new()
    {
        { "TestString", null },
        { "TestDateWithTime", null},
        { "TestDate", null },
        { "TestBool", "N" },
        { "TestInt" , null },
        { "TestLibId", null } ,
        { "WoGuidLibId" , null },
        { "SwcrLibId" , null } ,
        { "PersonOid" , null },
        { "DocumentId" , null },
        { "TestGuid" , "805519D70DB644B7BF99A0818CEA778E" }
    };

    [TestInitialize]
    public void Setup()
    {
        _oracleDBExecutorMock = Substitute.For<IPcs4Repository>();
        _syncUpdateHandler = new SqlUpdateStatementBuilder(_oracleDBExecutorMock);
        _nestedObject = new NestedSourceTestObject(_testGuid2);
        _sourceTestObject = new SourceTestObject(null, _testGuid, _testString, _testDate, _testDate2, _testBool, _testInt, _nestedObject, _woGuid, _swcrGuid, _personOid, _documentGuid);
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldReturnSqlStatmeent_WhenInputIsCorrect()
    {
        _oracleDBExecutorMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);
        var testObjectMappingConfig = new TestObjectMappingConfig();
        var (actualSqlUpdateStatement, actualSqlParams) = await _syncUpdateHandler.BuildAsync(testObjectMappingConfig, _sourceTestObject, default);
        Assert.AreEqual(_expectedSqlUpdateStatement, actualSqlUpdateStatement);

        foreach (var expectedParam in _expectedSqlParameters)
        {
            var actualParamValue = actualSqlParams.Get<object>(expectedParam.Key);
            if (actualParamValue != null && (actualParamValue is int || actualParamValue is long))
            {
                Assert.AreEqual(Convert.ToInt64(expectedParam.Value), Convert.ToInt64(actualParamValue));
            }
            else
            {
                Assert.AreEqual(expectedParam.Value, actualParamValue);
            }
        }
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldReturnSqlStatmeent_WhenSourceObjectHasNullValues()
    {
        _oracleDBExecutorMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        _sourceTestObject = new SourceTestObject(null, _testGuid, null, null, null, false, null, null, null, null, null, null);

        var testObjectMappingConfig = new TestObjectMappingConfig();
        var (actualSqlUpdateStatement, actualSqlParams) = await _syncUpdateHandler.BuildAsync(testObjectMappingConfig, _sourceTestObject, default);
        Assert.AreEqual(_expectedSqlUpdateStatement, actualSqlUpdateStatement);

        foreach (var expectedParam in _expectedSqlParametersNullValues)
        {
            var actualParamValue = actualSqlParams.Get<string>(expectedParam.Key);
            Assert.AreEqual(expectedParam.Value, actualParamValue);
        }
    }


    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldThrowException_WhenMissingProperty()
    {
        var testObjectMissingPropMappingConfig = new TestObjectMissingPropMappingConfig();

        var exception = await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _syncUpdateHandler.BuildAsync(testObjectMissingPropMappingConfig, _sourceTestObject, default);
        });

        Assert.AreEqual($"A property in configuration is missing in source object: PropMissing", exception.Message);
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldThrowException_WhenMissingConfigForObject()
    {
        var syncService = new SyncToPCS4Service(_oracleDBExecutorMock);

        var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await syncService.SyncObjectUpdateAsync(_sourceObjectNameMissingConfig, _sourceTestObject, default);
        });

        Assert.AreEqual($"Mapping is not implemented for source object with name '{_sourceObjectNameMissingConfig}'.", exception.Message);
    }
}
