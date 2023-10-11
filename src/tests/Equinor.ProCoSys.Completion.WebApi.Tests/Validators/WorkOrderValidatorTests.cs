using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.WebApi.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Validators;

[TestClass]
public class WorkOrderValidatorTests : ReadOnlyTestsBase
{
    private WorkOrder _openWorkOrder = null!;
    private WorkOrder _closedWorkOrder = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
            
        _openWorkOrder = new WorkOrder(TestPlantA, Guid.NewGuid(), "WorkOrder 1");
        _closedWorkOrder = new WorkOrder(TestPlantA, Guid.NewGuid(), "WorkOrder 2") { IsClosed = true };
        context.WorkOrders.Add(_openWorkOrder);
        context.WorkOrders.Add(_closedWorkOrder);

        context.SaveChangesAsync().Wait();
    }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenWorkOrderIsClosed()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);            
        var dut = new WorkOrderValidator(context);

        // Act
        var result = await dut.ExistsAsync(_closedWorkOrder.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenWorkOrderIsOpen()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new WorkOrderValidator(context);

        // Act
        var result = await dut.ExistsAsync(_openWorkOrder.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenWorkOrderNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);    
        var dut = new WorkOrderValidator(context);

        // Act
        var result = await dut.ExistsAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsClosed
    [TestMethod]
    public async Task IsClosed_ShouldReturnTrue_WhenWorkOrderIsClosed()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new WorkOrderValidator(context);

        // Act
        var result = await dut.IsClosedAsync(_closedWorkOrder.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsClosed_ShouldReturnFalse_WhenWorkOrderIsOpen()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new WorkOrderValidator(context);

        // Act
        var result = await dut.IsClosedAsync(_openWorkOrder.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsClosed_ShouldReturnFalse_WhenWorkOrderNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new WorkOrderValidator(context);

        // Act
        var result = await dut.IsClosedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
