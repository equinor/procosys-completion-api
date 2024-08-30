using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Projects;

[TestClass]
public class ProjectsControllerNegativeTests : TestBase
{
    #region GetAllPunchItems
    [TestMethod]
    public async Task GetAllPunchItems_AsAnonymous_ShouldReturnUnauthorized()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetAllPunchItems_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetAllPunchItems_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetAllPunchItems_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItems_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItems_AsWriter_ShouldReturnForbidden_WhenNoAccessToProject()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItems_AsWriter_ShouldReturnNotFound_WhenUnknownProject()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    #endregion
}
