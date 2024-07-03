using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class ContentAccessCheckerTests
{
    private readonly ProCoSys4CheckList _proCoSys4CheckList = new ("EQ", false, Guid.NewGuid());
    private readonly Guid _checkListGuid = Guid.NewGuid();
    private readonly Guid _punchItemGuid = Guid.NewGuid();
    private ContentAccessChecker _dut = null!;
    private IRestrictionRolesChecker _restrictionRolesCheckerMock = null!;
    private ICheckListCache _checkListCacheMock = null!;
    private IPunchItemHelper _punchItemHelperMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _restrictionRolesCheckerMock = Substitute.For<IRestrictionRolesChecker>();
        _checkListCacheMock = Substitute.For<ICheckListCache>();
        _punchItemHelperMock = Substitute.For<IPunchItemHelper>();

        _dut = new ContentAccessChecker(
            _restrictionRolesCheckerMock,
            _checkListCacheMock,
            _punchItemHelperMock);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnTrue_WhenUserHasNoRestrictions()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(true);
        
        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(Guid.Empty, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnTrue_WhenUserHasAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, default).Returns(_proCoSys4CheckList);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_proCoSys4CheckList.ResponsibleCode).Returns(true);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnFalse_WhenUserDoNotHaveAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, default).Returns(_proCoSys4CheckList);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_proCoSys4CheckList.ResponsibleCode).Returns(false);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldThrowException_WhenCheckListNotFound()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, default).Returns(null as ProCoSys4CheckList);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(
            () => _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid, default));
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListOwningPunchItemAsync_ShouldReturnTrue_WhenUserHasNoRestrictions()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(true);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListOwningPunchItemAsync(Guid.Empty, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListOwningPunchItemAsync_ShouldReturnTrue_WhenUserHasAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _punchItemHelperMock.GetCheckListGuidForPunchItemAsync(_punchItemGuid).Returns(_checkListGuid);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, default).Returns(_proCoSys4CheckList);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_proCoSys4CheckList.ResponsibleCode).Returns(true);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListOwningPunchItemAsync(_punchItemGuid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListOwningPunchItemAsync_ShouldReturnFalse_WhenUserDoNotHaveAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _punchItemHelperMock.GetCheckListGuidForPunchItemAsync(_punchItemGuid).Returns(_checkListGuid);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, default).Returns(_proCoSys4CheckList);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_proCoSys4CheckList.ResponsibleCode).Returns(false);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListOwningPunchItemAsync(_punchItemGuid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListOwningPunchItemAsync_ShouldThrowException_WhenCheckListGuidNotFoundForPunchItem()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _punchItemHelperMock.GetCheckListGuidForPunchItemAsync(_punchItemGuid).Returns(null as Guid?);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(
            () => _dut.HasCurrentUserAccessToCheckListOwningPunchItemAsync(_punchItemGuid, default));
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListOwningPunchItemAsync_ShouldThrowException_WhenCheckListNotFound()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _punchItemHelperMock.GetCheckListGuidForPunchItemAsync(_punchItemGuid).Returns(_checkListGuid);
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, default).Returns(null as ProCoSys4CheckList);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(
            () => _dut.HasCurrentUserAccessToCheckListOwningPunchItemAsync(_punchItemGuid, default));
    }
}
