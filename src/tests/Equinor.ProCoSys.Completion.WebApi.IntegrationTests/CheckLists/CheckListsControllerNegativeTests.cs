using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.CheckLists;

[TestClass]
public class CheckListsControllerNegativeTests : TestBase
{
    #region GetPunchItemsAsync
    [TestMethod]
    public async Task GetPunchItemsAsync_AsAnonymous_ShouldReturnUnauthorized()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItemsAsync_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemsAsync_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemsAsync_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemsAsync_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemsAsync_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    [TestMethod]
    public async Task GetPunchItemsAsync_AsReader_ShouldReturnForbidden_WhenNoAccessToProjectForCheckList()
        => await CheckListsControllerTestsHelper.GetPunchItemsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidInProjectWithoutAccess,
            HttpStatusCode.Forbidden);
    #endregion

    #region GetDuplicateInfoAsync
    [TestMethod]
    public async Task GetDuplicateInfoAsync_AsAnonymous_ShouldReturnUnauthorized()
        => await CheckListsControllerTestsHelper.GetDuplicateInfoAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetDuplicateInfoAsync_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await CheckListsControllerTestsHelper.GetDuplicateInfoAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetDuplicateInfoAsync_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await CheckListsControllerTestsHelper.GetDuplicateInfoAsync(
            UserType.Writer,
            TestFactory.Unknown,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetDuplicateInfoAsync_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await CheckListsControllerTestsHelper.GetDuplicateInfoAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetDuplicateInfoAsync_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await CheckListsControllerTestsHelper.GetDuplicateInfoAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            TestFactory.CheckListGuidNotRestricted,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetDuplicateInfoAsync_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await CheckListsControllerTestsHelper.GetDuplicateInfoAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    [TestMethod]
    public async Task GetDuplicateInfoAsync_AsReader_ShouldReturnForbidden_WhenNoAccessToProjectForCheckList()
        => await CheckListsControllerTestsHelper.GetDuplicateInfoAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidInProjectWithoutAccess,
            HttpStatusCode.Forbidden);
    #endregion
}
