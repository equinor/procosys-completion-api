using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

[TestClass]
public class PunchItemsControllerNegativeTests : TestBase
{
    private Guid _punchItemGuidUnderTest;
    private Guid _punchItemBGuidUnderTest;
    private Guid _checkListGuidUnderTest;
    private Guid _linkGuidUnderTest;
    private Guid _attachmentGuidUnderTest;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _punchItemGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItem.Guid;
        _punchItemBGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItemB.Guid;
        _checkListGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItem.CheckListGuid;
        _linkGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].LinkInPunchItemAGuid;
        _attachmentGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].AttachmentInPunchItemAGuid;
        TestFactory.Instance.SetupBlobStorageMock(new Uri("http://blah.blah.com"));

        await EnsureWrongRowVersionDifferFromCorrectRowVersion();
    }

    #region GetPunchItem
    [TestMethod]
    public async Task GetPunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest, 
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest, 
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess, 
            _punchItemGuidUnderTest, 
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItem_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess, 
            Guid.NewGuid(), 
            HttpStatusCode.NotFound);
    #endregion

    #region GetAllPunchItemsInProject
    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.Writer,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsWriter_ShouldReturnForbidden_WhenNoAccessToProject()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsWriter_ShouldReturnNotFound_WhenUnknownProject()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    #endregion

    #region CreatePunchItem
    [TestMethod]
    public async Task CreatePunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            "PA",
            "PunchItem1",
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            "PA",
            "PunchItem1",
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.BadRequest,
            expectedMessageOnBadRequest: "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            "PA",
            "PunchItem1",
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.BadRequest,
            expectedMessageOnBadRequest: "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            "PA",
            "PunchItem1",
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToProject()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithAccess,
            "PA",
            "PunchItem1",
            TestFactory.ProjectGuidWithoutAccess,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownProject()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            "PunchItem1",
            Guid.NewGuid(),
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.BadRequest,
            expectedMessageOnBadRequest: "Project with this guid does not exist");

    [TestMethod]
    public async Task CreatePunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            "PA",
            "PunchItem1",
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            "PA",
            "PunchItem1",
            TestFactory.ProjectGuidWithAccess,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.Forbidden);
    #endregion

    #region UpdatePunchItem
    [TestMethod]
    public async Task UpdatePunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new JsonPatchDocument(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdatePunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new JsonPatchDocument(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new JsonPatchDocument(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            new JsonPatchDocument(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            new JsonPatchDocument(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            new JsonPatchDocument(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task UpdatePunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            new JsonPatchDocument(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItem_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        var patchDocument = new JsonPatchDocument();
        patchDocument.Replace("Description", Guid.NewGuid().ToString());

        await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            patchDocument,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }

    #endregion

    #region UpdatePunchItemCategory
    [TestMethod]
    public async Task UpdatePunchItemCategory_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "PA",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdatePunchItemCategory_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "PA",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItemCategory_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "PA",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItemCategory_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "PA",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItemCategory_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "PA",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItemCategory_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            "PA",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task UpdatePunchItemCategory_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            "PA",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItemCategory_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        var guidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);
        Assert.AreNotEqual(guidAndRowVersion.RowVersion, TestFactory.WrongButValidRowVersion);

        await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            "PB",
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }

    #endregion

    #region DeletePunchItem
    [TestMethod]
    public async Task DeletePunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.NoPermissionUser, TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task DeletePunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItem_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        // Arrange
        var guidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess, 
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);
        Assert.AreNotEqual(guidAndRowVersion.RowVersion, TestFactory.WrongButValidRowVersion);

        // Act
        await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guidAndRowVersion.Guid,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }
    #endregion

    #region CreatePunchItemLink
    [TestMethod]
    public async Task CreatePunchItemLink_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchItemLink_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItemLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItemLink_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItemLink_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItemLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            "T",
            "U",
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task CreatePunchItemLink_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchItemLinks
    [TestMethod]
    public async Task GetPunchItemLinks_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItemLinks_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemLinks_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemLinks_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemLinks_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemLinks_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);
    #endregion

    #region UpdatePunchItemLink
    [TestMethod]
    public async Task UpdatePunchItemLink_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdatePunchItemLink_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItemLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItemLink_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItemLink_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItemLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            Guid.Empty, 
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task UpdatePunchItemLink_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItemLink_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    #endregion

    #region DeletePunchItemLink
    [TestMethod]
    public async Task DeletePunchItemLink_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunchItemLink_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchItemLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchItemLink_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItemLink_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItemLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            Guid.Empty, 
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task DeletePunchItemLink_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItemLink_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    #endregion

    #region CreatePunchItemComment
    [TestMethod]
    public async Task CreatePunchItemComment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "T",
            new List<string>(),
            new List<Guid>(),
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchItemComment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "T",
            new List<string>(),
            new List<Guid>(),
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItemComment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "T",
            new List<string>(),
            new List<Guid>(),
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItemComment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "T",
            new List<string>(),
            new List<Guid>(),
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItemComment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "T",
            new List<string>(),
            new List<Guid>(),
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItemComment_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            "T",
            new List<string>(),
            new List<Guid>(),
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task CreatePunchItemComment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            "T",
            new List<string>(),
            new List<Guid>(),
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchItemComments
    [TestMethod]
    public async Task GetPunchItemComments_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItemComments_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemComments_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemComments_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemComments_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemComments_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);
    #endregion

    #region UploadNewPunchItemAttachment
    [TestMethod]
    public async Task UploadNewPunchItemAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UploadNewPunchItemAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UploadNewPunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UploadNewPunchItemAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UploadNewPunchItemAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UploadNewPunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            new TestFile("T", "F"),
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task UploadNewPunchItemAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);
    #endregion

    #region UpdatePunchItemAttachment
    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            Guid.NewGuid().ToString(),
            new List<string>(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            Guid.NewGuid().ToString(),
            new List<string>(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            Guid.NewGuid().ToString(),
            new List<string>(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            Guid.NewGuid().ToString(),
            new List<string>(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            Guid.NewGuid().ToString(),
            new List<string>(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            _attachmentGuidUnderTest,
            Guid.NewGuid().ToString(),
            new List<string>(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownAttachment()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            new List<string>(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Attachment with this guid does not exist");

    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            Guid.NewGuid().ToString(),
            new List<string>(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchItemAttachments
    [TestMethod]
    public async Task GetPunchItemAttachments_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItemAttachments_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemAttachments_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemAttachments_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemAttachments_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemAttachments_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);
    #endregion

    #region GetPunchItemAttachmentDownloadUrl
    [TestMethod]
    public async Task GetPunchItemAttachmentDownloadUrl_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentDownloadUrlAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItemAttachmentDownloadUrl_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentDownloadUrlAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemAttachmentDownloadUrl_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentDownloadUrlAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemAttachmentDownloadUrl_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentDownloadUrlAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemAttachmentDownloadUrl_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentDownloadUrlAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemAttachmentDownloadUrl_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentDownloadUrlAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            _attachmentGuidUnderTest,
            HttpStatusCode.NotFound);

    [TestMethod]
    public async Task GetPunchItemAttachmentDownloadUrl_AsWriter_ShouldReturnNotFound_WhenUnknownAttachment()
        => await PunchItemsControllerTestsHelper.GetPunchItemAttachmentDownloadUrlAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);
    #endregion

    #region OverwriteExistingPunchItemAttachment
    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        var punchItemAttachmentUnderTest = await GetPunchItemAttachmentUnderTest();
        await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", punchItemAttachmentUnderTest.FileName),
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownFileName()
    {
        var punchItemAttachmentUnderTest = await GetPunchItemAttachmentUnderTest();
        await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", Guid.NewGuid().ToString()),
            punchItemAttachmentUnderTest.RowVersion,
            HttpStatusCode.BadRequest,
            "Punch item don't have an attachment with filename");
    }

    #endregion

    #region DeletePunchItemAttachment
    [TestMethod]
    public async Task DeletePunchItemAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownAttachment()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            Guid.NewGuid(),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Attachment with this guid does not exist");

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    #endregion

    #region ClearPunchItem
    [TestMethod]
    public async Task ClearPunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task ClearPunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task ClearPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task ClearPunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task ClearPunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task ClearPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task ClearPunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task ClearPunchItem_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);

    #endregion

    #region UnclearPunchItem
    [TestMethod]
    public async Task UnclearPunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UnclearPunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UnclearPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UnclearPunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UnclearPunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UnclearPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task UnclearPunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UnclearPunchItem_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);
        Assert.AreNotEqual(rowVersionAfterClear, TestFactory.WrongButValidRowVersion);

        // Act
        await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guid,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }

    #endregion

    #region RejectPunchItem
    [TestMethod]
    public async Task RejectPunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            Guid.NewGuid().ToString(),
            [],
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task RejectPunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            Guid.NewGuid().ToString(),
            [],
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task RejectPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            Guid.NewGuid().ToString(),
            [],
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task RejectPunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            Guid.NewGuid().ToString(),
            [],
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task RejectPunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            Guid.NewGuid().ToString(),
            [],
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task RejectPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            Guid.NewGuid().ToString(),
            [],
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task RejectPunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            Guid.NewGuid().ToString(),
            [],
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task RejectPunchItem_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);
        Assert.AreNotEqual(rowVersionAfterClear, TestFactory.WrongButValidRowVersion);

        // Act
        await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guid,
            Guid.NewGuid().ToString(),
            [],
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }

    #endregion

    #region VerifyPunchItem
    [TestMethod]
    public async Task VerifyPunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task VerifyPunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task VerifyPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task VerifyPunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task VerifyPunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task VerifyPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task VerifyPunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task VerifyPunchItem_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);
        Assert.AreNotEqual(rowVersionAfterClear, TestFactory.WrongButValidRowVersion);

        // Act
        await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guid,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }

    #endregion

    #region UnverifyPunchItem
    [TestMethod]
    public async Task UnverifyPunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UnverifyPunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UnverifyPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UnverifyPunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UnverifyPunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UnverifyPunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(), 
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task UnverifyPunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UnverifyPunchItem_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        // Arrange
        var (guid, rowVersionAfterVerify) = await PunchItemsControllerTestsHelper.CreateVerifiedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);
        Assert.AreNotEqual(rowVersionAfterVerify, TestFactory.WrongButValidRowVersion);

        // Act
        await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guid,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }

    #endregion

    #region GetHistory
    [TestMethod]
    public async Task GetPunchItemHistory_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchItemHistoryAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItemHistory_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemHistoryAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemHistory_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemHistoryAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemHistory_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemHistoryAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemHistory_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemHistoryAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemHistory_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.GetPunchItemHistoryAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);
    #endregion

    #region GetPunchItemsByCheckListGuid
    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchItemsByCheckListGuid(
            UserType.Anonymous,
            TestFactory.Unknown,
            _checkListGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemsByCheckListGuid(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _checkListGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemsByCheckListGuid(
            UserType.Writer,
            TestFactory.Unknown,
            _checkListGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemsByCheckListGuid(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _checkListGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchItemsByCheckListGuid(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _checkListGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.GetPunchItemsByCheckListGuid(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsReader_ShouldReturnForbidden_WhenNoAccessToCheckList()
        => await PunchItemsControllerTestsHelper.GetPunchItemsByCheckListGuid(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidRestrictedProject,
            HttpStatusCode.Forbidden);

    #endregion

    #region GetCheckListsByPunchItemGuid
    [TestMethod]
    public async Task GetCheckListsByPunchItemGuid_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetCheckListsByPunchItemGuid(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetCheckListsByPunchItemGuid_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
     => await PunchItemsControllerTestsHelper.GetCheckListsByPunchItemGuid(
         UserType.NoPermissionUser,
         TestFactory.Unknown,
         _punchItemGuidUnderTest,
         HttpStatusCode.BadRequest,
         "is not a valid plant");

    [TestMethod]
    public async Task GetCheckListsByPunchItemGuid_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetCheckListsByPunchItemGuid(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetCheckListsByPunchItemGuid_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetCheckListsByPunchItemGuid(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetCheckListsByPunchItemGuid_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetCheckListsByPunchItemGuid(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetCheckListsByPunchItemGuid_AsWriter_ShouldReturnNotFound_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.GetCheckListsByPunchItemGuid(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.NotFound);

    [TestMethod]
    public async Task GetCheckListsByPunchItemGuid_AsReader_ShouldReturnForbidden_WhenNoAccessToPunchItemProject()
        => await PunchItemsControllerTestsHelper.GetCheckListsByPunchItemGuid(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemBGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    private async Task EnsureWrongRowVersionDifferFromCorrectRowVersion()
    {
        var punchItemUnderTest = await GetPunchItemUnderTest();
        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchItemUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");

        var punchItemLinkUnderTest = await GetPunchItemLinkUnderTest();
        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchItemLinkUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");

        var punchItemAttachmentUnderTest = await GetPunchItemAttachmentUnderTest();

        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchItemAttachmentUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");
    }

    private async Task<AttachmentDto> GetPunchItemAttachmentUnderTest()
    {
        var punchItemAttachmentsUnderTest = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest);
        var punchAttachmentUnderTest = punchItemAttachmentsUnderTest.Single(p => p.Guid == _attachmentGuidUnderTest);
        return punchAttachmentUnderTest;
    }

    private async Task<LinkDto> GetPunchItemLinkUnderTest()
    {
        var punchItemLinksUnderTest = await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest);
        var punchLinkUnderTest = punchItemLinksUnderTest.Single(p => p.Guid == _linkGuidUnderTest);
        return punchLinkUnderTest;
    }

    private async Task<PunchItemDetailsDto> GetPunchItemUnderTest()
    {
        var punchItemUnderTest = await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest);
        return punchItemUnderTest;
    }
}
