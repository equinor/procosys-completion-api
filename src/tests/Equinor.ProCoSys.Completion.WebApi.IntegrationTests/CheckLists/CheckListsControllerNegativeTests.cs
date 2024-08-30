using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.CheckLists;

[TestClass]
public class CheckListsControllerNegativeTests : TestBase
{
    private Guid _checkListGuidUnderTest;

    [TestInitialize]
    public void TestInitialize()
        => _checkListGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItemA.CheckListGuid;

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsAnonymous_ShouldReturnUnauthorized()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _checkListGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _checkListGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _checkListGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _checkListGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _checkListGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsReader_ShouldReturnForbidden_WhenNoAccessToProjectForCheckList()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidInProjectWithoutAccess,
            HttpStatusCode.Forbidden);
}
