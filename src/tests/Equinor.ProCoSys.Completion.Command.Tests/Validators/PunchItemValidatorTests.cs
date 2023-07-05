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
    private PunchItem _punchItemInOpenProject;
    private PunchItem _punchItemInClosedProject;
    private PunchItem _unclearedPunchItem;
    private PunchItem _clearedPunchItem;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _punchItemInOpenProject = new PunchItem(TestPlantA, _projectA, "x1");
        _punchItemInClosedProject = new PunchItem(TestPlantA, _closedProjectC, "x2");
        _unclearedPunchItem = _punchItemInOpenProject;
        _clearedPunchItem = new PunchItem(TestPlantA, _projectA, "x3");
        _clearedPunchItem.Clear(_currentPerson);

        context.PunchItems.Add(_punchItemInOpenProject);
        context.PunchItems.Add(_punchItemInClosedProject);
        context.PunchItems.Add(_clearedPunchItem);

        context.SaveChangesAsync().Wait();
    }

    #region Exists
    [TestMethod]
    public async Task Exists_ShouldReturnTrue_WhenPunchItemExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);            
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.ExistsAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Exists_ShouldReturnFalse_WhenPunchItemNotExist()
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

    #region ProjectOwningPunchItemIsClosed
    [TestMethod]
    public async Task ProjectOwningPunchItemIsClosed_ShouldReturnTrue_WhenPunchItemIsInClosedProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.ProjectOwningPunchItemIsClosedAsync(_punchItemInClosedProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ProjectOwningPunchItemIsClosed_ShouldReturnFalse_WhenPunchItemIsInOpenProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.ProjectOwningPunchItemIsClosedAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ProjectOwningPunchItemIsClosed_ShouldReturnFalse_WhenPunchItemNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.ProjectOwningPunchItemIsClosedAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region TagOwningPunchItemIsVoided
    // todo #103935 
    [TestMethod]
    public async Task TagOwningPunchItemIsVoided_ShouldReturnTrue_WhenPunchItemOwnedByVoidedTag()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        // var result = await dut.TagOwningPunchItemIsVoidedAsync(, default);

        // Assert
        // Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task TagOwningPunchItemIsVoided_ShouldReturnFalse_WhenPunchItemOwnedByNonvoidedTag()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        // var result = await dut.TagOwningPunchItemIsVoidedAsync(, default);

        // Assert
        // Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task TagOwningPunchItemIsVoided_ShouldReturnFalse_WhenPunchItemNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        // var result = await dut.TagOwningPunchItemIsVoidedAsync(, default);

        // Assert
        //Assert.IsFalse(result);
    }
    #endregion

    #region IsReadyToBeCleared
    [TestMethod]
    public async Task IsReadyToBeCleared_ShouldReturnTrue_WhenNotCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.IsReadyToBeClearedAsync(_unclearedPunchItem.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsReadyToBeCleared_ShouldReturnFalse_WhenCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.IsReadyToBeClearedAsync(_clearedPunchItem.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsReadyToBeCleared_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemValidator(context);

        // Act
        var result = await dut.IsReadyToBeClearedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
#endregion
}
