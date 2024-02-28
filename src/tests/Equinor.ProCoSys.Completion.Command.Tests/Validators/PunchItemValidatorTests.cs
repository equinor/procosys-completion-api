using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Command.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class PunchItemValidatorTests : ReadOnlyTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private PunchItem _punchItemPb = null!;
    private PunchItem _punchItemInOpenProject = null!;
    private PunchItem _punchItemInClosedProject = null!;
    private PunchItem _notClearedPunchItem = null!;
    private PunchItem _clearedButNotVerifiedPunchItem = null!;
    private PunchItem _verifiedPunchItem = null!;
    private ICheckListValidator _checkListValidatorMock = null!;
    private Project _projectA = null!;
    private Project _closedProjectC = null!;
    private Person _currentPerson = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        _checkListValidatorMock = Substitute.For<ICheckListValidator>();

        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _currentPerson = context.Persons.Single(p => p.Guid == CurrentUserOid);
        _projectA = context.Projects.Single(p => p.Id == _projectAId[_testPlant]);
        _closedProjectC = context.Projects.Single(p => p.Id == _closedProjectCId[_testPlant]);

        var raisedByOrg = context.Library.Single(l => l.Id == _raisedByOrgId[_testPlant]);
        var clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId[_testPlant]);

        _punchItemInOpenProject = new PunchItem(_testPlant, _projectA, Guid.NewGuid(), Category.PB, "x1", raisedByOrg, clearingByOrg);
        _punchItemPb = _punchItemInOpenProject;
        _punchItemInClosedProject = new PunchItem(_testPlant, _closedProjectC, Guid.NewGuid(), Category.PB, "x2", raisedByOrg, clearingByOrg);
        _notClearedPunchItem = _punchItemInOpenProject;
        _clearedButNotVerifiedPunchItem = new PunchItem(_testPlant, _projectA, Guid.NewGuid(), Category.PB, "x3", raisedByOrg, clearingByOrg);
        _clearedButNotVerifiedPunchItem.Clear(_currentPerson);
        _verifiedPunchItem = new PunchItem(_testPlant, _projectA, Guid.NewGuid(), Category.PB, "x4", raisedByOrg, clearingByOrg);
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
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);            
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.ExistsAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Exists_ShouldReturnFalse_WhenPunchItemNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);    
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

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
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.ProjectOwningPunchItemIsClosedAsync(_punchItemInClosedProject.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ProjectOwningPunchItemIsClosed_ShouldReturnFalse_WhenPunchItemIsInOpenProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.ProjectOwningPunchItemIsClosedAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ProjectOwningPunchItemIsClosed_ShouldReturnFalse_WhenPunchItemNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.ProjectOwningPunchItemIsClosedAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region TagOwningPunchItemIsVoided
    [TestMethod]
    public async Task TagOwningPunchItemIsVoided_ShouldReturnTrue_WhenPunchItemOwnedByVoidedTag()
    {
        // Arrange
        _checkListValidatorMock.TagOwningCheckListIsVoidedAsync(_punchItemInOpenProject.CheckListGuid).Returns(true);
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

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
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.TagOwningPunchItemIsVoidedAsync(_punchItemInOpenProject.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task TagOwningPunchItemIsVoided_ShouldReturnFalse_WhenPunchItemNotExist()
    {
        // Arrange
        _checkListValidatorMock.TagOwningCheckListIsVoidedAsync(_punchItemInOpenProject.CheckListGuid).Returns(false);
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.TagOwningPunchItemIsVoidedAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsCleared
    [TestMethod]
    public async Task IsCleared_ShouldReturnFalse_WhenNotCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.IsClearedAsync(_notClearedPunchItem.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsCleared_ShouldReturnTrue_WhenCleared()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.IsClearedAsync(_clearedButNotVerifiedPunchItem.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsCleared_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.IsClearedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region IsVerified
    [TestMethod]
    public async Task IsVerified_ShouldReturnFalse_WhenClearedButNotVerified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.IsVerifiedAsync(_clearedButNotVerifiedPunchItem.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsVerified_ShouldReturnTrue_WhenVerified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.IsVerifiedAsync(_verifiedPunchItem.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsVerified_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.IsVerifiedAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region HasCategory
    [TestMethod]
    public async Task HasCategory_ShouldReturnTrue_WhenPunchItemHasSameCategory()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.HasCategoryAsync(_punchItemPb.Guid, Category.PB, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCategory_ShouldReturnFalse_WhenPunchItemHasOtherCategory()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.HasCategoryAsync(_punchItemPb.Guid, Category.PA, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region CurrentUserIsVerifier
    [TestMethod]
    public async Task CurrentUserIsVerifier_ShouldReturnFalse_WhenClearedButNotVerified()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.CurrentUserIsVerifierAsync(_clearedButNotVerifiedPunchItem.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CurrentUserIsVerifier_ShouldReturnTrue_WhenVerifiedByCurrentUser()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.CurrentUserIsVerifierAsync(_verifiedPunchItem.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CurrentUserIsVerifier_ShouldReturnFalse_WhenVerifiedByOtherThanCurrentUser()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);
        _currentUserProviderMock.GetCurrentUserOid().Returns(Guid.NewGuid());

        // Act
        var result = await dut.CurrentUserIsVerifierAsync(_verifiedPunchItem.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CurrentUserIsVerifier_ShouldReturnFalse_WhenPunchNotExist()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _checkListValidatorMock, _currentUserProviderMock);

        // Act
        var result = await dut.CurrentUserIsVerifierAsync(Guid.Empty, default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
