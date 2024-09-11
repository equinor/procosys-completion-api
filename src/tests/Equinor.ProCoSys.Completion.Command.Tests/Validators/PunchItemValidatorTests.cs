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
    private PunchItem _punchItem = null!;
    private PunchItem _clearedButNotVerifiedPunchItem = null!;
    private PunchItem _verifiedPunchItem = null!;
    private Project _projectA = null!;
    private readonly string _knownExternalItemNo = "ExtNo";
    private Person _currentPerson = null!;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _currentPerson = context.Persons.Single(p => p.Guid == CurrentUserOid);
        _projectA = context.Projects.Single(p => p.Id == _projectAId[TestPlantA]);

        var raisedByOrg = context.Library.Single(l => l.Id == _raisedByOrgId[TestPlantA]);
        var clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId[TestPlantA]);

        _punchItem = new(TestPlantA, _projectA, Guid.NewGuid(), Category.PB, "x1", raisedByOrg, clearingByOrg)
        {
            ExternalItemNo = _knownExternalItemNo
        };
        _clearedButNotVerifiedPunchItem = new PunchItem(TestPlantA, _projectA, Guid.NewGuid(), Category.PB, "x3", raisedByOrg, clearingByOrg);
        _clearedButNotVerifiedPunchItem.Clear(_currentPerson);
        _verifiedPunchItem = new PunchItem(TestPlantA, _projectA, Guid.NewGuid(), Category.PB, "x4", raisedByOrg, clearingByOrg);
        _verifiedPunchItem.Clear(_currentPerson);
        _verifiedPunchItem.Verify(_currentPerson);

        context.PunchItems.Add(_punchItem);
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
        var result = await dut.ExistsAsync(_punchItem.Guid, default);

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

    #region ExternalItemNoExistsInProject
    [TestMethod]
    public async Task ExternalItemNoExistsInProject_ShouldReturnTrue_WhenPunchItemExistWithExternalItemNoInProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _currentUserProviderMock);

        // Act
        var result = await dut.ExternalItemNoExistsInProjectAsync(_knownExternalItemNo, _punchItem.Project.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExternalItemNoExistsInProject_ShouldReturnFalse_WhenPunchItemNotExistWithExternalItemNoInProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _currentUserProviderMock);

        // Act
        var result = await dut.ExternalItemNoExistsInProjectAsync("X", _punchItem.Project.Guid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ExternalItemNoExistsInProject_ShouldReturnFalse_WhenPunchItemExistWithExternalItemNoInOtherProject()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new PunchItemValidator(context, _currentUserProviderMock);

        // Act
        var result = await dut.ExternalItemNoExistsInProjectAsync(_knownExternalItemNo, Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
