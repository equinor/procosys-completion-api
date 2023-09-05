using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class ContentAccessCheckerTests
{
    private const string Plant = "X";
    private readonly ProCoSys4CheckList _proCoSys4CheckList = new ("EQ", false, Guid.NewGuid());
    private readonly Guid _checkListGuid = Guid.NewGuid();
    private ContentAccessChecker _dut;
    private IRestrictionRolesChecker _restrictionRolesCheckerMock;
    private ICheckListCache _checkListCacheMock;
    private IPlantProvider _plantProviderMock;

    [TestInitialize]
    public void Setup()
    {
        _restrictionRolesCheckerMock = Substitute.For<IRestrictionRolesChecker>();
        _checkListCacheMock = Substitute.For<ICheckListCache>();
        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant.Returns(Plant);
            
        _dut = new ContentAccessChecker(
            _restrictionRolesCheckerMock,
            _checkListCacheMock,
            _plantProviderMock);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnTrue_WhenUserHasNoRestrictions()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(true);
        
        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(Guid.Empty);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnTrue_WhenUserHasAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(Plant, _checkListGuid).Returns(_proCoSys4CheckList);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_proCoSys4CheckList.ResponsibleCode).Returns(true);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnFalse_WhenUserDoNotHaveAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(Plant, _checkListGuid).Returns(_proCoSys4CheckList);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_proCoSys4CheckList.ResponsibleCode).Returns(false);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldThrowException_WhenCheckListDoNotExists()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListCacheMock.GetCheckListAsync(Plant, _checkListGuid).Returns(null as ProCoSys4CheckList);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(
            () => _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid));
    }
}
