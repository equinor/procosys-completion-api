using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class SqlUpdateStatementBuilderTests : SqlStatementBuilderTestsBase
{
    private SqlUpdateStatementBuilder _dut;

    private readonly string _expectedSqlUpdateStatement =
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
    public void Setup() => _dut = new SqlUpdateStatementBuilder(_pcs4Repository);

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldReturnSqlStatment_WhenInputIsCorrect()
    {
        // Arrange
        _pcs4Repository.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        // Act
        var (actualSqlUpdateStatement, actualSqlParams) = await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObject, null!, default);

        // Assert
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
    public async Task BuildSqlUpdateStatement_ShouldReturnSqlStatment_WhenSourceObjectHasNullValues()
    {
        // Arrange
        _pcs4Repository.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);
        _sourceTestObject = new SourceTestObject(null, TestGuid, null, null, null, false, null, null!, null, null, null, null);

        // Act
        var (actualSqlUpdateStatement, actualSqlParams) = await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObject, null!, default);

        // Assert
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
        // Act
        var exception = await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.BuildAsync(_testObjectMissingPropMappingConfig, _sourceTestObject, null!, default);
        });

        // Assert
        Assert.AreEqual($"A property in configuration is missing in source object: PropMissing", exception.Message);
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldThrowException_WhenMissingConfigForObject()
    {
        // Arrange
        var syncService = new SyncToPCS4Service(_pcs4Repository);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await syncService.SyncObjectUpdateAsync(SourceObjectNameMissingConfig, _sourceTestObject, null!, default);
        });

        // Assert
        Assert.AreEqual($"Mapping is not implemented for source object with name '{SourceObjectNameMissingConfig}'.", exception.Message);
    }
}
