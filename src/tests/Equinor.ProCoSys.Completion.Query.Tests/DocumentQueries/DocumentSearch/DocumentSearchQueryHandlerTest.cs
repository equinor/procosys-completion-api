using System.Linq;
using Equinor.ProCoSys.Completion.Infrastructure;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Query.DocumentQueries;

namespace Equinor.ProCoSys.Completion.Query.Tests.DocumentQueries.DocumentSearch;

[TestClass]
public class DocumentSearchQueryHandlerTest : ReadOnlyTestsBase
{
    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenNoMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new DocumentSearchQueryHandler(context);
        DocumentSearchQuery query = new(Guid.NewGuid().ToString());

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnDocument_WhenMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new DocumentSearchQueryHandler(context);
        DocumentSearchQuery query = new(DocumentNo);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(1, result.Data.Count());
    }
    
    [TestMethod]
    public async Task Handle_ShouldReturnOnlyNonVoidedDocuments()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var voidedDocument = new Document(TestPlantA, Guid.NewGuid(), DocumentNo + "voided"){IsVoided = true};
        context.Documents.Add(voidedDocument);
        await context.SaveChangesAsync();
        
        var dut = new DocumentSearchQueryHandler(context);
        DocumentSearchQuery query = new(DocumentNo);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        
        Assert.AreEqual(1, result.Data.Count());
        Assert.IsTrue(result.Data.First().No == DocumentNo);
    }
}

