using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.WebApi.Validators.PunchItemValidators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Validators;

[TestClass]
public class PunchItemValidatorTests : ReadOnlyTestsBase
{
    private PunchItem _punchItemInOpenProject = null!;
    private PunchItem _punchItemInClosedProject = null!;
    private PunchItem _notClearedPunchItem = null!;
    private PunchItem _clearedButNotVerifiedPunchItem = null!;
    private PunchItem _verifiedPunchItem = null!;
    private ICheckListValidator _checkListValidatorMock = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        _checkListValidatorMock = Substitute.For<ICheckListValidator>();

        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var raisedByOrg = context.Library.Single(l => l.Id == _raisedByOrgId);
        var clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId);

        _punchItemInOpenProject = new PunchItem(TestPlantA, _projectA, Guid.NewGuid(), "x1", raisedByOrg, clearingByOrg);
        _punchItemInClosedProject = new PunchItem(TestPlantA, _closedProjectC, Guid.NewGuid(), "x2", raisedByOrg, clearingByOrg);
        _notClearedPunchItem = _punchItemInOpenProject;
        _clearedButNotVerifiedPunchItem = new PunchItem(TestPlantA, _projectA, Guid.NewGuid(), "x3", raisedByOrg, clearingByOrg);
        _clearedButNotVerifiedPunchItem.Clear(_currentPerson);
        _verifiedPunchItem = new PunchItem(TestPlantA, _projectA, Guid.NewGuid(), "x4", raisedByOrg, clearingByOrg);
        _verifiedPunchItem.Clear(_currentPerson);
        _verifiedPunchItem.Verify(_currentPerson);

        context.PunchItems.Add(_punchItemInOpenProject);
        context.PunchItems.Add(_punchItemInClosedProject);
        context.PunchItems.Add(_clearedButNotVerifiedPunchItem);
        context.PunchItems.Add(_verifiedPunchItem);

        context.SaveChangesAsync().Wait();
    }

    #region Exists
    [TestMethod]
    public async Task Exists_ShouldReturnTrue_WhenPunchItemExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);            
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.ExistsAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Exists_ShouldReturnFalse_WhenPunchItemNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);    
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

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
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.ProjectOwningPunchItemIsClosedAsync(_punchItemInClosedProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ProjectOwningPunchItemIsClosed_ShouldReturnFalse_WhenPunchItemIsInOpenProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.ProjectOwningPunchItemIsClosedAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ProjectOwningPunchItemIsClosed_ShouldReturnFalse_WhenPunchItemNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

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
        _checkListValidatorMock.TagOwningCheckListIsVoidedAsync(_punchItemInOpenProject.CheckListGuid).Returns(true);
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.TagOwningPunchItemIsVoidedAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task TagOwningPunchItemIsVoided_ShouldReturnFalse_WhenPunchItemOwnedByNonVoidedTag()
    {
        // Arrange
        _checkListValidatorMock.TagOwningCheckListIsVoidedAsync(_punchItemInOpenProject.CheckListGuid).Returns(false);
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.TagOwningPunchItemIsVoidedAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsCleared
    [TestMethod]
    public async Task IsCleared_ShouldReturnFalse_WhenNotCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.IsClearedAsync(_notClearedPunchItem.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsCleared_ShouldReturnTrue_WhenCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.IsClearedAsync(_clearedButNotVerifiedPunchItem.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsCleared_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.IsClearedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsVerified
    [TestMethod]
    public async Task IsVerified_ShouldReturnFalse_WhenNotCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.IsVerifiedAsync(_notClearedPunchItem.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsVerified_ShouldReturnFalse_WhenClearedButNotVerified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.IsVerifiedAsync(_clearedButNotVerifiedPunchItem.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsVerified_ShouldReturnTrue_WhenVerified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.IsVerifiedAsync(_verifiedPunchItem.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsVerified_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemValidator(context, _checkListValidatorMock);

        // Act
        var result = await dut.IsVerifiedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
