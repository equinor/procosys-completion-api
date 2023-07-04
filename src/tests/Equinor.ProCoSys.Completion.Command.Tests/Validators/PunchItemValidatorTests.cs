using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class PunchItemValidatorTests : ReadOnlyTestsBase
{
    private PunchItem _punchInOpenProject;
    private PunchItem _punchInClosedProject;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _punchInOpenProject = new PunchItem(TestPlantA, _projectA, "x1");
        _punchInClosedProject = new PunchItem(TestPlantA, _closedProjectC, "x2");
        context.PunchItems.Add(_punchInOpenProject);
        context.PunchItems.Add(_punchInClosedProject);

        context.SaveChangesAsync().Wait();
    }

    #region PunchExists
    [TestMethod]
    public async Task PunchExists_ShouldReturnTrue_WhenPunchExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);            
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.ExistsAsync(_punchInOpenProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task PunchExists_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);    
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.ExistsAsync(Guid.Empty, default);

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
        var dut = new PunchItemValidator(context);

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
        var dut = new PunchItemValidator(context);

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
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.ProjectOwningPunchIsClosedAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
