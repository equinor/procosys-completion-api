using System.Linq;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.SWCRQueries;
using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;

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
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnSWCR_WhenMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new SWCRSearchQueryHandler(context);
        SWCRSearchQuery _query = new(SWCRNo.ToString());

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
    }
    
    
    [TestMethod]
    public async Task Handle_ShouldNotReturnVoidedResults()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        context.SWCRs.Add(new SWCR(TestPlantA, Guid.NewGuid(), 11){IsVoided = true});
        await context.SaveChangesAsync();
        
        var dut = new SWCRSearchQueryHandler(context);
        var searchPhrase = SWCRNo.ToString();//equates to "1"
        SWCRSearchQuery query = new(searchPhrase); //expect to find swcr with id 1, but not 11, as 11 is voided

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.IsTrue(result.First().No == SWCRNo);
    }
}
