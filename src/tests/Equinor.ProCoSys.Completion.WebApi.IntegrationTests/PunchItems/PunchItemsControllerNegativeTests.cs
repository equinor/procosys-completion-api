using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

[TestClass]
public class PunchItemsControllerNegativeTests : TestBase
{
    private Guid _punchGuidUnderTest;
    private Guid _linkGuidUnderTest;
    private Guid _attachmentGuidUnderTest;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _punchGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].PunchAGuid;
        _linkGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].LinkInPunchAGuid;
        _attachmentGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].AttachmentInPunchAGuid;

        await EnsureWrongRowVersionDifferFromCorrectRowVersion();
    }

    #region GetPunch
    [TestMethod]
    public async Task GetPunch_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunch_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunch_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest, 
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunch_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest, 
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunch_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess, 
            _punchGuidUnderTest, 
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunch_AsWriter_ShouldReturnNotFound_WhenUnknownPunch()
        => await PunchItemsControllerTestsHelper.GetPunchAsync(
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

    #region CreatePunch
    [TestMethod]
    public async Task CreatePunch_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.CreatePunchAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunch_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunch_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunch_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunch_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunch_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            "Punch1",
            TestFactory.ProjectGuidWithAccess,
            HttpStatusCode.Forbidden);
    #endregion

    #region UpdatePunch
    [TestMethod]
    public async Task UpdatePunch_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UpdatePunchAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdatePunch_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunch_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunch_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunch_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunch_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UpdatePunchAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunch_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.UpdatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);

    #endregion

    #region DeletePunch
    [TestMethod]
    public async Task DeletePunch_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.DeletePunchAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunch_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchAsync(
            UserType.NoPermissionUser, TestFactory.Unknown,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunch_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunch_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunch_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunch_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.DeletePunchAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunch_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        var idAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess);
        // Act

        await PunchItemsControllerTestsHelper.DeletePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            idAndRowVersion.Guid,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }
    #endregion

    #region CreatePunchLink
    [TestMethod]
    public async Task CreatePunchLink_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.CreatePunchLinkAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchLink_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchLinkAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchLink_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchLink_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchLink_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchLinkAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchLinks
    [TestMethod]
    public async Task GetPunchLinks_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchLinksAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchLinks_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchLinksAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchLinks_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchLinksAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchLinks_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchLinksAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchLinks_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    #region UpdatePunchLink
    [TestMethod]
    public async Task UpdatePunchLink_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdatePunchLink_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunchLink_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchLink_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchLink_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunchLink_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    #endregion

    #region DeletePunchLink
    [TestMethod]
    public async Task DeletePunchLink_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunchLink_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchLink_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchLink_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchLink_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchLink_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    #endregion

    #region CreatePunchComment
    [TestMethod]
    public async Task CreatePunchComment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchComment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchCommentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchComment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchComment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchCommentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchComment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchComment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchComments
    [TestMethod]
    public async Task GetPunchComments_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchCommentsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchComments_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchCommentsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchComments_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchCommentsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchComments_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchCommentsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchComments_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchCommentsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    #region UploadNewPunchAttachment
    [TestMethod]
    public async Task UploadNewPunchAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchAttachments
    [TestMethod]
    public async Task GetPunchAttachments_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchAttachments_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchAttachments_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchAttachments_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchAttachments_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchAttachmentDownloadUrl
    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    #region OverwriteExistingPunchAttachment
    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        var punchAttachmentUnderTest = await GetPunchAttachmentUnderTest();
        await PunchItemsControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            new TestFile("T", punchAttachmentUnderTest.FileName),
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    }

    #endregion

    #region DeletePunchAttachment
    [TestMethod]
    public async Task DeletePunchAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunchAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchAttachment_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchItemsControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.WrongButValidRowVersion,
            HttpStatusCode.Conflict);
    #endregion

    private async Task EnsureWrongRowVersionDifferFromCorrectRowVersion()
    {
        var punchUnderTest = await GetPunchUnderTest();
        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");

        var punchLinkUnderTest = await GetPunchLinkUnderTest();
        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchLinkUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");

        var punchAttachmentUnderTest = await GetPunchAttachmentUnderTest();

        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchAttachmentUnderTest.RowVersion,
            "Incorrect test data. TestFactory.WrongButValidRowVersion need do differ actual RowVersion");
    }

    private async Task<AttachmentDto> GetPunchAttachmentUnderTest()
    {
        var punchAttachmentsUnderTest = await PunchItemsControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest);
        var punchAttachmentUnderTest = punchAttachmentsUnderTest.Single(p => p.Guid == _attachmentGuidUnderTest);
        return punchAttachmentUnderTest;
    }

    private async Task<LinkDto> GetPunchLinkUnderTest()
    {
        var punchLinksUnderTest = await PunchItemsControllerTestsHelper.GetPunchLinksAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest);
        var punchLinkUnderTest = punchLinksUnderTest.Single(p => p.Guid == _linkGuidUnderTest);
        return punchLinkUnderTest;
    }

    private async Task<PunchDetailsDto> GetPunchUnderTest()
    {
        var punchUnderTest = await PunchItemsControllerTestsHelper.GetPunchAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest);
        return punchUnderTest;
    }
}
