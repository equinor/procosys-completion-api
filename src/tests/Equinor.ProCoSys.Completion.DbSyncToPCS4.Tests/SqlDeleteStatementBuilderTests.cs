using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class SqlDeleteStatementBuilderTests : SqlStatementBuilderTestsBase
{
    private SqlDeleteStatementBuilder _dut;

    private readonly string _expectedSqlDeleteStatement = "delete from TestTargetTable where TestGuid = :TestGuid";

    private readonly Dictionary<string, object> _expectedSqlParameters = new()
    {
        { "TestGuid" , "805519D70DB644B7BF99A0818CEA778E" }
    };

    [TestInitialize]
    public void Setup()
    {
        _dut = new SqlDeleteStatementBuilder(_oracleDBExecutorMock);
    }

    [TestMethod]
    public async Task BuildSqlDeleteStatement_ShouldReturnSqlStatmeent_WhenInputIsCorrect()
    {
        // Arrange
        _oracleDBExecutorMock.ValueLookupNumberAsync(Arg.Any<string>(), Arg.Any<DynamicParameters>(), default).Returns(123456789);

        // Act
        var (actualSqlUpdateStatement, actualSqlParams) = await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObject, default);

        // Assert
        Assert.AreEqual(_expectedSqlDeleteStatement, actualSqlUpdateStatement);

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
    public async Task BuildSqlUpdateStatement_ShouldThrowException_WhenMissingPrimarykey()
    {
        // Act
        var exception = await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.BuildAsync(_testObjectMappingConfig, _sourceTestObjectMissingPrimaryKey, default);
        });

        // Assert
        Assert.AreEqual($"A property in configuration is missing in source object: TestGuid", exception.Message);
    }

    [TestMethod]
    public async Task BuildSqlUpdateStatement_ShouldThrowException_WhenMissingConfigForObject()
    {
        // Arrange
        var syncService = new SyncToPCS4Service(_oracleDBExecutorMock);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
        {
            await syncService.SyncObjectUpdateAsync(_sourceObjectNameMissingConfig, _sourceTestObject, default);
        });

        // Assert
        Assert.AreEqual($"Mapping is not implemented for source object with name '{_sourceObjectNameMissingConfig}'.", exception.Message);
    }
}
