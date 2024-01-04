using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class SyncInsertHandlerTests
{
    private IPcs4Repository _oracleDBExecutorMock;

    private readonly string _testOnlyForInsert = "testOnlyForInsert";
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
    private SourceTestObject _sourceTestObject;
    private SqlInsertStatementBuilder _syncInsertHandler;

    private readonly string _expectedSqlInsertStatement =
        "insert into TestTargetTable ( TestOnlyForInsert, TestFixedValue, TestGuid, TestString, TestDateWithTime, TestDate, TestBool, TestInt, TestLibId, WoGuidLibId, SwcrLibId, PersonOid, DocumentId ) " +
        "values ( :TestOnlyForInsert, 'Fixed value', :TestGuid, :TestString, :TestDateWithTime, :TestDate, :TestBool, :TestInt, :TestLibId, :WoGuidLibId, :SwcrLibId, :PersonOid, :DocumentId )";

    private readonly Dictionary<string, object> _expectedSqlParameters = new()
    {
        { "TestOnlyForInsert", "testOnlyForInsert" },
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
        { "TestBool", "N" },
        { "TestGuid" , "805519D70DB644B7BF99A0818CEA778E" }
    };

    [TestInitialize]
    public void Setup()
    {
        _oracleDBExecutorMock = Substitute.For<IPcs4Repository>();
        _syncInsertHandler = new SqlInsertStatementBuilder(_oracleDBExecutorMock);
        _nestedObject = new NestedSourceTestObject(_testGuid2);
        _sourceTestObject = new SourceTestObject(_testOnlyForInsert, _testGuid, _testString, _testDate, _testDate2, _testBool, _testInt, _nestedObject, _woGuid, _swcrGuid, _personOid, _documentGuid);

    }

    [TestMethod]
    public async Task BuildSqlInsertStatement_ShouldReturnSqlStatmeent_WhenInputIsCorrect()
    {
        _oracleDBExecutorMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        var testObjectMappingConfig = new TestObjectMappingConfig();
        var (actualSqlInsertStatement, actualSqlParams) = await _syncInsertHandler.BuildAsync(testObjectMappingConfig, _sourceTestObject, default);
        Assert.AreEqual(_expectedSqlInsertStatement, actualSqlInsertStatement);

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
    public async Task BuildSqlInsertStatement_ShouldReturnSqlStatment_WhenSourceObjectHasNullValues()
    {
        var sourceTestObject = new SourceTestObject(null, _testGuid, null, null, null, false, null, null, null, null, null, null);

        _oracleDBExecutorMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        var expectedSqlInsertStatement = "insert into TestTargetTable ( TestFixedValue, TestGuid, TestBool ) " +
            "values ( 'Fixed value', :TestGuid, :TestBool )";

        var testObjectMappingConfig = new TestObjectMappingConfig();

        var (actualSqlInsertStatement, actualSqlParams) = await _syncInsertHandler.BuildAsync(testObjectMappingConfig, sourceTestObject, default);

        Assert.AreEqual(expectedSqlInsertStatement, actualSqlInsertStatement);

        foreach (var expectedParam in _expectedSqlParametersNullValues)
        {
            var actualParamValue = actualSqlParams.Get<string>(expectedParam.Key);
            Assert.AreEqual(expectedParam.Value, actualParamValue);
        }
    }


    [TestMethod]
    public async Task BuildSqlInsertStatement_ShouldThrowException_WhenMissingProperty()
    {
        var testObjectMissingPropMappingConfig = new TestObjectMissingPropMappingConfig();

        var exception = await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _syncInsertHandler.BuildAsync(testObjectMissingPropMappingConfig, _sourceTestObject, default);
        });

        Assert.AreEqual($"A property in configuration is missing in source object: PropMissing", exception.Message);
    }

    [TestMethod]
    public async Task BuildSqlInsertStatement_ShouldThrowException_WhenMissingConfigForObject()
    {
        var syncService = new SyncToPCS4Service(_oracleDBExecutorMock);
        var sourceObjectNameMissingConfig = "NotInConfiguration";

        var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await syncService.SyncInsertAsync(sourceObjectNameMissingConfig, _sourceTestObject, default);
        });

        Assert.AreEqual($"Mapping is not implemented for source object with name '{sourceObjectNameMissingConfig}'.", exception.Message);
    }
}
