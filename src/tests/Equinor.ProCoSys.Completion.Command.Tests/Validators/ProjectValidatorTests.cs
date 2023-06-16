using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class ProjectValidatorTests : ReadOnlyTestsBase
{
    private Project _openProject;
    private Project _closedProject;
    private Punch _punchInOpenProject;
    private Punch _punchInClosedProject;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            
        _openProject = new Project(TestPlantA, Guid.NewGuid(), "Project 1", "D1");
        _closedProject = new Project(TestPlantA, Guid.NewGuid(), "Project 2", "D2") { IsClosed = true };
        context.Projects.Add(_openProject);
        context.Projects.Add(_closedProject);

        _punchInOpenProject = new Punch(TestPlantA, _openProject, "x1");
        _punchInClosedProject = new Punch(TestPlantA, _closedProject, "x2");
        context.Punches.Add(_punchInOpenProject);
        context.Punches.Add(_punchInClosedProject);

        context.SaveChangesAsync().Wait();
    }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenProjectIsClosed()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);            
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.ExistsAsync(_closedProject.Name, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenProjectIsOpen()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.ExistsAsync(_openProject.Name, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenProjectNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);    
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.ExistsAsync("P X", default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsClosed
    [TestMethod]
    public async Task IsClosed_ShouldReturnTrue_WhenProjectIsClosed()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosed(_closedProject.Name, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsClosed_ShouldReturnFalse_WhenProjectIsOpen()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosed(_openProject.Name, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsClosed_ShouldReturnFalse_WhenProjectNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosed("P X", default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsClosedForPunch
    [TestMethod]
    public async Task IsClosedForPunch_ShouldReturnTrue_WhenPunchIsInClosedProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosedForPunch(_punchInClosedProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsClosedForPunch_ShouldReturnFalse_WhenPunchIsInOpenProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosedForPunch(_punchInOpenProject.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsClosedForPunch_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new ProjectValidator(context);

        // Act
        var result = await dut.IsClosedForPunch(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
