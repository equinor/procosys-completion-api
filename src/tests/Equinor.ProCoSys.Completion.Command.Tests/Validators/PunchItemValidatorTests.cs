using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Validators;

[TestClass]
public class PunchItemValidatorTests : ReadOnlyTestsBase
{
    private PunchItem _punchItemInOpenProject = null!;
    private PunchItem _punchItemInClosedProject = null!;
    private PunchItem _clearedButNotVerifiedPunchItem = null!;
    private PunchItem _verifiedPunchItem = null!;
    private Project _projectA = null!;
    private Project _closedProjectC = null!;
    private Person _currentPerson = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _currentPerson = context.Persons.Single(p => p.Guid == CurrentUserOid);
        _projectA = context.Projects.Single(p => p.Id == _projectAId[TestPlantA]);
        _closedProjectC = context.Projects.Single(p => p.Id == _closedProjectCId[TestPlantA]);

        var raisedByOrg = context.Library.Single(l => l.Id == _raisedByOrgId[TestPlantA]);
        var clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId[TestPlantA]);

        _punchItemInOpenProject = new PunchItem(TestPlantA, _projectA, Guid.NewGuid(), Category.PB, "x1", raisedByOrg, clearingByOrg);
        _punchItemInClosedProject = new PunchItem(TestPlantA, _closedProjectC, Guid.NewGuid(), Category.PB, "x2", raisedByOrg, clearingByOrg);
        _clearedButNotVerifiedPunchItem = new PunchItem(TestPlantA, _projectA, Guid.NewGuid(), Category.PB, "x3", raisedByOrg, clearingByOrg);
        _clearedButNotVerifiedPunchItem.Clear(_currentPerson);
        _verifiedPunchItem = new PunchItem(TestPlantA, _projectA, Guid.NewGuid(), Category.PB, "x4", raisedByOrg, clearingByOrg);
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
        var dut = new PunchItemValidator(context, _currentUserProviderMock);

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
        var dut = new PunchItemValidator(context, _currentUserProviderMock);

        // Act
        var result = await dut.ExistsAsync(Guid.Empty, default);

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
        var dut = new PunchItemValidator(context, _currentUserProviderMock);

        // Act
        var result = dut.CurrentUserIsVerifier(_clearedButNotVerifiedPunchItem);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CurrentUserIsVerifier_ShouldReturnTrue_WhenVerifiedByCurrentUser()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _currentUserProviderMock);

        // Act
        var result = dut.CurrentUserIsVerifier(_verifiedPunchItem);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CurrentUserIsVerifier_ShouldReturnFalse_WhenVerifiedByOtherThanCurrentUser()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _currentUserProviderMock);
        _currentUserProviderMock.GetCurrentUserOid().Returns(Guid.NewGuid());

        // Act
        var result = dut.CurrentUserIsVerifier(_verifiedPunchItem);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
