using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.WebApi.MainApi;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class ContentAccessCheckerTests
{
    private readonly string _plant = "X";
    private readonly string _responsible = "EQ";
    private readonly Guid _checkListGuid = Guid.NewGuid();
    private ContentAccessChecker _dut;
    private Mock<IRestrictionRolesChecker> _restrictionRolesCheckerMock;
    private Mock<ICheckListApiService> _checkListApiServiceMock;
    private Mock<IPlantProvider> _plantProviderMock;

    [TestInitialize]
    public void Setup()
    {
        _restrictionRolesCheckerMock = new Mock<IRestrictionRolesChecker>();
        _checkListApiServiceMock = new Mock<ICheckListApiService>();
        _plantProviderMock = new Mock<IPlantProvider>();
        _plantProviderMock.Setup(p => p.Plant).Returns(_plant);
            
        _dut = new ContentAccessChecker(
            _restrictionRolesCheckerMock.Object,
            _checkListApiServiceMock.Object,
            _plantProviderMock.Object);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnTrue_WhenUserHasNoRestrictions()
    {
        // Arrange
        _restrictionRolesCheckerMock.Setup(r => r.HasCurrentUserExplicitNoRestrictions()).Returns(true);
        
        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(Guid.Empty);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnTrue_WhenUserHasAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.Setup(r => r.HasCurrentUserExplicitNoRestrictions()).Returns(false);
        _checkListApiServiceMock.Setup(c => c.GetCheckListAsync(_plant, _checkListGuid)).ReturnsAsync(_responsible);
        _restrictionRolesCheckerMock.Setup(r => r.HasCurrentUserExplicitAccessToContent(_responsible)).Returns(true);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldReturnFalse_WhenUserDoNotHaveAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.Setup(r => r.HasCurrentUserExplicitNoRestrictions()).Returns(false);
        _checkListApiServiceMock.Setup(c => c.GetCheckListAsync(_plant, _checkListGuid)).ReturnsAsync(_responsible);
        _restrictionRolesCheckerMock.Setup(r => r.HasCurrentUserExplicitAccessToContent(_responsible)).Returns(false);

        // Act
        var result = await _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task HasCurrentUserAccessToCheckListAsync_ShouldThrowInValidCheckListException_WhenCheckListDoNotExists()
    {
        // Arrange
        _restrictionRolesCheckerMock.Setup(r => r.HasCurrentUserExplicitNoRestrictions()).Returns(false);
        _checkListApiServiceMock.Setup(c => c.GetCheckListAsync(_plant, _checkListGuid)).ReturnsAsync((string)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<InValidCheckListException>(
            () => _dut.HasCurrentUserAccessToCheckListAsync(_checkListGuid));
    }
}
