using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

[TestClass]
public class PunchItemsControllerNegativeTests : TestBase
{
    private Guid _punchItemGuidUnderTest;
    private Guid _checkListGuidUnderTest;
    private Guid _attachmentGuidUnderTest;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _punchItemGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItemA.Guid;
        _checkListGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItemA.CheckListGuid;
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
            expectedStatusCode: HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownCheckList()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            "PunchItem1",
            Guid.NewGuid(),
            Guid.Empty,
            Guid.Empty,
            expectedStatusCode: HttpStatusCode.BadRequest,
            expectedMessageOnBadRequest: "Check list with this guid does not exist");

    [TestMethod]
    public async Task CreatePunchItem_AsWriter_ShouldReturnBadRequest_WhenSetToLongDescription()
    {
        var description = new string('x', PunchItem.DescriptionLengthMax + 1);
        await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            description,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid,
            expectedStatusCode: HttpStatusCode.BadRequest,
            expectedMessageOnBadRequest:
            $"The length of 'Description' must be {PunchItem.DescriptionLengthMax} characters or fewer. You entered {description.Length} characters.");
    }

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
            expectedStatusCode: HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task CreatePunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            "PA",
            "PunchItem1",
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
    public async Task UpdatePunchItem_AsWriter_ShouldReturnBadRequest_WhenSetToLongDescription()
    {
        var patchDocument = new JsonPatchDocument();
        var description = new string('x', PunchItem.DescriptionLengthMax + 1);
        patchDocument.Replace("Description", description);
        await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            patchDocument,
            TestFactory.AValidRowVersion,
            HttpStatusCode.BadRequest,
            $"Can't assign value to property Description. Length is {description.Length}. Length must be minimum 1 and maximum {PunchItem.DescriptionLengthMax}");
    }

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

    #region DuplicatePunchItem
    [TestMethod]
    public async Task DuplicatePunchItem_AsAnonymous_ShouldReturnUnauthorized()
        => await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.Anonymous,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            [_checkListGuidUnderTest],
            false,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task DuplicatePunchItem_AsNoPermissionUser_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            [_checkListGuidUnderTest],
            false,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DuplicatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPlant()
        => await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.Writer,
            TestFactory.Unknown,
            _punchItemGuidUnderTest,
            [_checkListGuidUnderTest],
            false,
            HttpStatusCode.BadRequest,
            "is not a valid plant");

    [TestMethod]
    public async Task DuplicatePunchItem_AsNoPermissionUser_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.NoPermissionUser,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            [_checkListGuidUnderTest],
            false,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DuplicatePunchItem_AsWriter_ShouldReturnForbidden_WhenNoAccessToPlant()
        => await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithoutAccess,
            _punchItemGuidUnderTest,
            [_checkListGuidUnderTest],
            false,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task DuplicatePunchItem_AsWriter_ShouldReturnBadRequest_WhenUnknownPunchItem()
        => await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid(),
            [_checkListGuidUnderTest],
            false,
            HttpStatusCode.BadRequest,
            "Punch item with this guid does not exist");

    [TestMethod]
    public async Task DuplicatePunchItem_AsReader_ShouldReturnForbidden_WhenPermissionMissing()
        => await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest,
            [_checkListGuidUnderTest],
            false,
            HttpStatusCode.Forbidden);

    #endregion

    private async Task EnsureWrongRowVersionDifferFromCorrectRowVersion()
    {
        var punchItemUnderTest = await GetPunchItemUnderTest();
        Assert.AreNotEqual(
            TestFactory.WrongButValidRowVersion,
            punchItemUnderTest.RowVersion,
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

    private async Task<PunchItemDetailsDto> GetPunchItemUnderTest()
    {
        var punchItemUnderTest = await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest);
        return punchItemUnderTest;
    }
}
