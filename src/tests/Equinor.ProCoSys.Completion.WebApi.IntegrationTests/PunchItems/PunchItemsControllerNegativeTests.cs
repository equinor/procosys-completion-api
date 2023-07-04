using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

[TestClass]
public class PunchItemsControllerNegativeTests : TestBase
{
    private Guid _punchItemGuidUnderTest;
    private Guid _linkGuidUnderTest;
    private Guid _attachmentGuidUnderTest;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _punchItemGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].PunchAGuid;
        _linkGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].LinkInPunchAGuid;
        _attachmentGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].AttachmentInPunchAGuid;

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
    public async Task GetPunchItem_AsWriter_ShouldReturnNotFound_WhenUnknownPunch()
        => await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess, 
            Guid.Empty, 
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
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsWriter_ShouldReturnForbidden_WhenNoAccessToProject()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsWriter_ShouldReturnBadRequest_WhenUnknownProject()
        => await PunchItemsControllerTestsHelper.GetAllPunchItemsInProjectAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            HttpStatusCode.BadRequest);
    #endregion

    #region CreatePunchItem
    [TestMethod]
    public async Task CreatePunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            "Punch1",
            TestFactory.ProjectGuidWithAccess,
            HttpStatusCode.Forbidden);
    #endregion

    #region UpdatePunchItem
    [TestMethod]
    public async Task UpdatePunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "Punch item updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdatePunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "Punch item updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "Punch item updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "Punch item updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "Punch item updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            "Punch item updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchItem_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            "Punch item updated",
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);

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
        var idAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess);
        // Act

        await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            idAndRowVersion.Guid,
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
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchItemComment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "T",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItemComment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            "T",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchItemComment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "T",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItemComment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            "T",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItemComment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            "T",
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
    public async Task UploadNewPunchItemAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", "F"),
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
        var punchAttachmentUnderTest = await GetPunchItemAttachmentUnderTest();
        await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            new TestFile("T", punchAttachmentUnderTest.FileName),
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
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

    private async Task EnsureWrongRowVersionDifferFromCorrectRowVersion()
    {
        var punchUnderTest = await GetPunchItemUnderTest();
        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");

        var punchLinkUnderTest = await GetPunchItemLinkUnderTest();
        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchLinkUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");

        var punchAttachmentUnderTest = await GetPunchItemAttachmentUnderTest();

        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchAttachmentUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");
    }

    private async Task<AttachmentDto> GetPunchItemAttachmentUnderTest()
    {
        var punchAttachmentsUnderTest = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest);
        var punchAttachmentUnderTest = punchAttachmentsUnderTest.Single(p => p.Guid == _attachmentGuidUnderTest);
        return punchAttachmentUnderTest;
    }

    private async Task<LinkDto> GetPunchItemLinkUnderTest()
    {
        var punchLinksUnderTest = await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest);
        var punchLinkUnderTest = punchLinksUnderTest.Single(p => p.Guid == _linkGuidUnderTest);
        return punchLinkUnderTest;
    }

    private async Task<PunchItemDetailsDto> GetPunchItemUnderTest()
    {
        var punchUnderTest = await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest);
        return punchUnderTest;
    }
}
