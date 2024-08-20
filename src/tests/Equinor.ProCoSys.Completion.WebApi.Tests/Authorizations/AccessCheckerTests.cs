using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class AccessCheckerTests
{
    private readonly ProCoSys4CheckList _proCoSys4CheckList = new("EQ", false, Guid.NewGuid());
    private readonly Guid _checkListGuid = Guid.NewGuid();
    private AccessChecker _dut = null!;
    private IRestrictionRolesChecker _restrictionRolesCheckerMock = null!;
    private ICheckListCache _checkListCacheMock = null!;
    private readonly CheckListDetailsDto _checkListDetailsDto = new(Guid.NewGuid(), "R", false, Guid.NewGuid());

    [TestInitialize]
    public void Setup()
    {
        _restrictionRolesCheckerMock = Substitute.For<IRestrictionRolesChecker>();
        _checkListCacheMock = Substitute.For<ICheckListCache>();

        _dut = new AccessChecker(_restrictionRolesCheckerMock, _checkListCacheMock);
    }

    [TestMethod]
    public async Task HasCurrentUserWriteAccessToCheckListAsync_ShouldReturnTrue_WhenUserHasNoRestrictions()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(true);

        // Act
        var result = await _dut.HasCurrentUserWriteAccessToCheckListAsync(Guid.Empty, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToCheckList_ShouldReturnTrue_WhenUserHasNoRestrictions()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(true);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToCheckList(_checkListDetailsDto);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserWriteAccessToCheckListAsync_ShouldReturnTrue_WhenUserHasAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, Arg.Any<CancellationToken>()).Returns(_proCoSys4CheckList);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_proCoSys4CheckList.ResponsibleCode).Returns(true);

        // Act
        var result = await _dut.HasCurrentUserWriteAccessToCheckListAsync(_checkListGuid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToCheckList_ShouldReturnTrue_WhenUserHasAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_checkListDetailsDto.ResponsibleCode).Returns(true);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToCheckList(_checkListDetailsDto);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserWriteAccessToCheckListAsync_ShouldReturnFalse_WhenUserDoNotHaveAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, Arg.Any<CancellationToken>()).Returns(_proCoSys4CheckList);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_proCoSys4CheckList.ResponsibleCode).Returns(false);

        // Act
        var result = await _dut.HasCurrentUserWriteAccessToCheckListAsync(_checkListGuid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToCheckList_ShouldReturnFalse_WhenUserDoNotHaveAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_checkListDetailsDto.ResponsibleCode).Returns(false);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToCheckList(_checkListDetailsDto);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task HasCurrentUserWriteAccessToCheckListAsync_ShouldThrowException_WhenCheckListNotFound()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, Arg.Any<CancellationToken>()).Returns(null as ProCoSys4CheckList);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(
            () => _dut.HasCurrentUserWriteAccessToCheckListAsync(_checkListGuid, default));
    }

}
