using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Punches;

[TestClass]
public class PunchesControllerNegativeTests : TestBase
{
    private Guid _punchGuidUnderTest;
    private Guid _linkGuidUnderTest;
    private Guid _attachmentGuidUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _punchGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].PunchAGuid;
        _linkGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].LinkInPunchAGuid;
        _attachmentGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].AttachmentInPunchAGuid;
    }

    #region GetPunch
    [TestMethod]
    public async Task GetPunch_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.GetPunchAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunch_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunch_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest, 
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunch_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest, 
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunch_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess, 
            _punchGuidUnderTest, 
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunch_AsWriter_ShouldReturnNotFound_WhenUnknownPunch()
        => await PunchesControllerTestsHelper.GetPunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess, 
            Guid.Empty, 
            HttpStatusCode.NotFound);
    #endregion

    #region GetPunchesInProject
    [TestMethod]
    public async Task GetPunchesInProject_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.GetAllPunchesInProjectAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchesInProject_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetAllPunchesInProjectAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchesInProject_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetAllPunchesInProjectAsync(
            UserType.Writer,
            TestFactory.Unknown,
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchesInProject_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetAllPunchesInProjectAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchesInProject_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetAllPunchesInProjectAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchesInProject_AsWriter_ShouldReturnForbidden_WhenNoAccessToProject()
        => await PunchesControllerTestsHelper.GetAllPunchesInProjectAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithoutAccess,
            HttpStatusCode.Forbidden);
    #endregion

    #region CreatePunch
    [TestMethod]
    public async Task CreatePunch_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunch_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunch_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.Unknown,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunch_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunch_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            "Punch1",
            Guid.Empty,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunch_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            "Punch1",
            TestFactory.ProjectGuidWithAccess,
            HttpStatusCode.Forbidden);
    #endregion

    #region UpdatePunch
    [TestMethod]
    public async Task UpdatePunch_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.UpdatePunchAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdatePunch_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.UpdatePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunch_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.UpdatePunchAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UpdatePunch_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.UpdatePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunch_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.UpdatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunch_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.UpdatePunchAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            "Punch updated",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UpdatePunch_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
        => await PunchesControllerTestsHelper.UpdatePunchAsync(
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
        => await PunchesControllerTestsHelper.DeletePunchAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunch_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.DeletePunchAsync(
            UserType.NoPermissionUser, TestFactory.Unknown,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunch_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.DeletePunchAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunch_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.DeletePunchAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunch_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.DeletePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunch_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.DeletePunchAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunch_AsWriter_ShouldReturnConflict_WhenWrongRowVersion()
    {
        var idAndRowVersion = await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess);
        // Act

        await PunchesControllerTestsHelper.DeletePunchAsync(
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
        => await PunchesControllerTestsHelper.CreatePunchLinkAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchLink_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.CreatePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.CreatePunchLinkAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchLink_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.CreatePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchLink_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.CreatePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "T",
            "U",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchLink_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.CreatePunchLinkAsync(
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
        => await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchLinks_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchLinks_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchLinks_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchLinks_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    #region UpdatePunchLink
    [TestMethod]
    public async Task UpdatePunchLink_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.UpdatePunchLinkAsync(
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
        => await PunchesControllerTestsHelper.UpdatePunchLinkAsync(
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
        => await PunchesControllerTestsHelper.UpdatePunchLinkAsync(
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
        => await PunchesControllerTestsHelper.UpdatePunchLinkAsync(
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
        => await PunchesControllerTestsHelper.UpdatePunchLinkAsync(
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
        => await PunchesControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            "T",
            "U",
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);
    #endregion

    #region DeletePunchLink
    [TestMethod]
    public async Task DeletePunchLink_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunchLink_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.DeletePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchLink_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchLink_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.DeletePunchLinkAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchLink_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchLink_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _linkGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);
    #endregion

    #region CreatePunchComment
    [TestMethod]
    public async Task CreatePunchComment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreatePunchComment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.CreatePunchCommentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchComment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task CreatePunchComment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.CreatePunchCommentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchComment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchComment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            "T",
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchComments
    [TestMethod]
    public async Task GetPunchComments_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.GetPunchCommentsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchComments_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchCommentsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchComments_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchCommentsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchComments_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchCommentsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchComments_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchCommentsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    #region UploadNewPunchAttachment
    [TestMethod]
    public async Task UploadNewPunchAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task UploadNewPunchAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchAttachments
    [TestMethod]
    public async Task GetPunchAttachments_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchAttachments_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchAttachments_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchAttachments_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchAttachments_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    #region GetPunchAttachmentDownloadUrl
    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            HttpStatusCode.Forbidden);
    #endregion

    #region OverwriteExistingPunchAttachment
    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            new TestFile("T", "F"),
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);
    #endregion

    #region DeletePunchAttachment
    [TestMethod]
    public async Task DeletePunchAttachment_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchesControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DeletePunchAttachment_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchAttachment_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchesControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DeletePunchAttachment_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchAttachment_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchesControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DeletePunchAttachment_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchesControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchGuidUnderTest,
            _attachmentGuidUnderTest,
            TestFactory.AValidRowVersion,
            HttpStatusCode.Forbidden);
    #endregion
}
