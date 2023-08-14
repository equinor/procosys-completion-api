using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.WebApi.MainApi;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class ContentAccessCheckerTests
{
    private const string Plant = "X";
    private const string Responsible = "EQ";
    private readonly Guid _checkListGuid = Guid.NewGuid();
    private ContentAccessChecker _dut;
    private IRestrictionRolesChecker _restrictionRolesCheckerMock;
    private ICheckListApiService _checkListApiServiceMock;
    private IPlantProvider _plantProviderMock;

    [TestInitialize]
    public void Setup()
    {
        _restrictionRolesCheckerMock = Substitute.For<IRestrictionRolesChecker>();
        _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant.Returns(Plant);
            
        _dut = new ContentAccessChecker(
            _restrictionRolesCheckerMock,
            _checkListApiServiceMock,
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
        _checkListApiServiceMock.GetCheckListAsync(Plant, _checkListGuid).Returns(Responsible);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(Responsible).Returns(true);

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
        _checkListApiServiceMock.GetCheckListAsync(Plant, _checkListGuid).Returns(Responsible);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(Responsible).Returns(false);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldThrowInValidCheckListException_WhenCheckListDoNotExists()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _checkListApiServiceMock.GetCheckListAsync(Plant, _checkListGuid).Returns((string)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<InValidCheckListException>(
            () => _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid));
    }
}
