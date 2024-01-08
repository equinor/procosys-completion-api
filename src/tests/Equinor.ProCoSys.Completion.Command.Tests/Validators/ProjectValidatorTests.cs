using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Command.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class ProjectValidatorTests : ReadOnlyTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private Project _openProject = null!;
    private Project _closedProject = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);
            
        _openProject = new Project(_testPlant, Guid.NewGuid(), "Project 1", "D1");
        _closedProject = new Project(_testPlant, Guid.NewGuid(), "Project 2", "D2") { IsClosed = true };
        context.Projects.Add(_openProject);
        context.Projects.Add(_closedProject);

        context.SaveChangesAsync().Wait();
    }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenProjectIsClosed()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);            
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.ExistsAsync(_closedProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenProjectIsOpen()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.ExistsAsync(_openProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenProjectNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);    
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.ExistsAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsClosed
    [TestMethod]
    public async Task IsClosed_ShouldReturnTrue_WhenProjectIsClosed()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosedAsync(_closedProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsClosed_ShouldReturnFalse_WhenProjectIsOpen()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosedAsync(_openProject.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsClosed_ShouldReturnFalse_WhenProjectNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
