﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Projects;

[TestClass]
public class ProjectsControllerNegativeTests : TestBase
{
    #region GetAllPunchItemsAsync
    [TestMethod]
    public async Task GetAllPunchItemsAsync_AsAnonymous_ShouldReturnUnauthorized()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetAllPunchItemsAsync_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetAllPunchItemsAsync_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetAllPunchItemsAsync_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsAsync_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsAsync_AsWriter_ShouldReturnForbidden_WhenNoAccessToProject()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsAsync_AsWriter_ShouldReturnNotFound_WhenUnknownProject()
        => await ProjectsControllerTestsHelper.GetAllPunchItemsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    #endregion

    #region SearchCheckListsAsync
    [TestMethod]
    public async Task SearchCheckListsAsync_AsAnonymous_ShouldReturnUnauthorized()
        => await ProjectsControllerTestsHelper.SearchCheckListsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task SearchCheckListsAsync_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await ProjectsControllerTestsHelper.SearchCheckListsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task SearchCheckListsAsync_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await ProjectsControllerTestsHelper.SearchCheckListsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task SearchCheckListsAsync_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await ProjectsControllerTestsHelper.SearchCheckListsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task SearchCheckListsAsync_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await ProjectsControllerTestsHelper.SearchCheckListsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task SearchCheckListsAsync_AsWriter_ShouldReturnForbidden_WhenNoAccessToProject()
        => await ProjectsControllerTestsHelper.SearchCheckListsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task SearchCheckListsAsync_AsWriter_ShouldReturnNotFound_WhenUnknownProject()
        => await ProjectsControllerTestsHelper.SearchCheckListsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    #endregion
}
