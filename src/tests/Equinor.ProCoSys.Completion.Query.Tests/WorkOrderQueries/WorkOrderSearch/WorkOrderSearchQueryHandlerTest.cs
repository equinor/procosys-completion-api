using System.Linq;
using Equinor.ProCoSys.Completion.Infrastructure;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.WorkOrderQueries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

namespace Equinor.ProCoSys.Completion.Query.Tests.WorkOrderQueries.WorkOrderSearch;

[TestClass]
public class WorkOrderSearchQueryHandlerTest : ReadOnlyTestsBase
{
    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
    }

    [TestMethod]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new WorkOrderSearchQueryHandler(context);
        WorkOrderSearchQuery query = new(Guid.NewGuid().ToString());

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task Handle_ShouldReturnWorkOrder_WhenMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new WorkOrderSearchQueryHandler(context);
        WorkOrderSearchQuery query = new(WorkOrderNo);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
    }
    
    [TestMethod]
    public async Task Handle_ShouldExcludeVoidedWorkOrders()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        
        var woNo = "SomeWorkOrderNo";
        var workOrder = new WorkOrder("PCS$PlantA", Guid.NewGuid(), woNo) { IsVoided = true };
        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync();
        
        var dut = new WorkOrderSearchQueryHandler(context);
        WorkOrderSearchQuery query = new(woNo);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }
}

