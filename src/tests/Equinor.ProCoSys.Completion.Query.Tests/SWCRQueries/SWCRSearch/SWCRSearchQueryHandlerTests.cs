using System.Linq;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.SWCRQueries;
using System;

namespace Equinor.ProCoSys.Completion.Query.Tests.SWCRQueries.SWCRSearch;

[TestClass]
public class SWCRSearchQueryHandlerTests : ReadOnlyTestsBase
{
    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenNoMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new SWCRSearchQueryHandler(context);
        SWCRSearchQuery _query = new(Guid.NewGuid().ToString());

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnSWCR_WhenMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new SWCRSearchQueryHandler(context);
        SWCRSearchQuery _query = new(ReadOnlyTestsBase.SWCRNo.ToString());

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(1, result.Data.Count());
    }
}
