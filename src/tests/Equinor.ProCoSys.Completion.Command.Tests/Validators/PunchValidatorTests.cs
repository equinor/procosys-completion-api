using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class PunchValidatorTests : ReadOnlyTestsBase
{
    private Punch _punchInOpenProject;
    private Punch _punchInClosedProject;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _punchInOpenProject = new Punch(TestPlantA, _projectA, "x1");
        _punchInClosedProject = new Punch(TestPlantA, _closedProjectC, "x2");
        context.Punches.Add(_punchInOpenProject);
        context.Punches.Add(_punchInClosedProject);

        context.SaveChangesAsync().Wait();
    }

    #region PunchExists
    [TestMethod]
    public async Task PunchExists_ShouldReturnTrue_WhenPunchExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);            
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.PunchExistsAsync(_punchInOpenProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task PunchExists_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);    
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.PunchExistsAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region ProjectOwningPunchIsClosed
    [TestMethod]
    public async Task ProjectOwningPunchIsClosedAsync_ShouldReturnTrue_WhenPunchIsInClosedProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.ProjectOwningPunchIsClosedAsync(_punchInClosedProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ProjectOwningPunchIsClosedAsync_ShouldReturnFalse_WhenPunchIsInOpenProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.ProjectOwningPunchIsClosedAsync(_punchInOpenProject.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ProjectOwningPunchIsClosedAsync_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchValidator(context);

        // Act
        var result = await dut.ProjectOwningPunchIsClosedAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
