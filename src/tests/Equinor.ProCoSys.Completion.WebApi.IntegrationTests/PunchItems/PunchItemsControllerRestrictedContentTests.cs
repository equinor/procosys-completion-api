using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

[TestClass]
public class PunchItemsControllerRestrictedContentTests : TestBase
{
    private readonly string _plantWithAccess = TestFactory.PlantWithAccess;
    private readonly Guid _projectGuidWithAccess = TestFactory.ProjectGuidWithAccess;

    #region CreatePunchItem
    [TestMethod]
    public async Task CreatePunchItem_AsRestrictedWriter_ShouldReturnForbidden()
        => await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            "PA",
            "PunchItem1",
            _projectGuidWithAccess,
            TestFactory.CheckListGuidRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid,
            DateTime.UtcNow,
            TestFactory.PriorityGuid,
            TestFactory.SortingGuid,
            TestFactory.TypeGuid,
            expectedStatusCode: HttpStatusCode.Forbidden);
    #endregion

    #region UpdatePunchItem
    [TestMethod]
    public async Task UpdatePunchItem_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var guidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guidAndRowVersion.Guid,
            new JsonPatchDocument(),
            guidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region UpdatePunchItemCategory
    [TestMethod]
    public async Task UpdatePunchItemCategory_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var guidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guidAndRowVersion.Guid,
            "PB",
            guidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region DeletePunchItem
    [TestMethod]
    public async Task DeletePunchItem_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var guidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guidAndRowVersion.Guid,
            guidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region CreatePunchItemLink
    [TestMethod]
    public async Task CreatePunchItemLink_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var guidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guidAndRowVersion.Guid,
            "T",
            "U",
            HttpStatusCode.Forbidden);
    }

    #endregion

    #region UpdatePunchItemLink
    [TestMethod]
    public async Task UpdatePunchItemLink_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var (punchItemGuidAndRowVersion, linkGuidAndRowVersion) = await CreatePunchItemLinkInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            "T",
            "U",
            punchItemGuidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }

    #endregion

    #region DeletePunchItemLink
    [TestMethod]
    public async Task DeletePunchItemLink_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var (punchItemGuidAndRowVersion, linkGuidAndRowVersion) = await CreatePunchItemLinkInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region CreatePunchItemComment
    [TestMethod]
    public async Task CreatePunchItemComment_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var guidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guidAndRowVersion.Guid,
            "T",
            new List<string>(),
            new List<Guid>(),
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region UploadNewPunchItemAttachment
    [TestMethod]
    public async Task UploadNewPunchItemAttachment_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var guidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guidAndRowVersion.Guid,
            new TestFile("T", "F"),
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region UpdatePunchItemAttachment
    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion)
            = await UploadNewPunchItemAttachmentInRestrictedCheckListAsync(Guid.NewGuid().ToString());

        // Act
        await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            Guid.NewGuid().ToString(),
            new List<string>(),
            attachmentGuidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region OverwriteExistingPunchItemAttachment
    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion)
            = await UploadNewPunchItemAttachmentInRestrictedCheckListAsync(fileName);

        // Act
        await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            new TestFile("T", fileName),
            attachmentGuidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }

    #endregion

    #region DeletePunchItemAttachment
    [TestMethod]
    public async Task DeletePunchItemAttachment_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion)
            = await UploadNewPunchItemAttachmentInRestrictedCheckListAsync(Guid.NewGuid().ToString());

        // Act
        await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region ClearPunchItem
    [TestMethod]
    public async Task ClearPunchItem_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var guidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        // Act
        await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guidAndRowVersion.Guid,
            guidAndRowVersion.RowVersion,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region UnclearPunchItem
    [TestMethod]
    public async Task UnclearPunchItem_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        // Act
        await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guid,
            rowVersionAfterClear,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region RejectPunchItem
    [TestMethod]
    public async Task RejectPunchItem_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        // Act
        await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guid,
            Guid.NewGuid().ToString(),
            rowVersionAfterClear,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region VerifyPunchItem
    [TestMethod]
    public async Task VerifyPunchItem_AsRestrictedWriter_ShouldReturnForbidden()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        // Act
        await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guid,
            rowVersionAfterClear,
            HttpStatusCode.Forbidden);
    }
    #endregion

    #region UnverifyPunchItem
    [TestMethod]
    public async Task UnverifyPunchItem_AsRestrictedWriter_ShouldReturnForbidden()
    {
        var (guid, rowVersionAfterVerify) = await PunchItemsControllerTestsHelper.CreateVerifiedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.ProjectGuidWithAccess,
            TestFactory.CheckListGuidRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.RestrictedWriter,
            _plantWithAccess,
            guid,
            rowVersionAfterVerify,
            HttpStatusCode.Forbidden);
    }
    #endregion

    private async Task<GuidAndRowVersion> CreatePunchItemInRestrictedCheckListAsync()
    {
        var guidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            _plantWithAccess,
            "PA",
            "PunchItem1",
            _projectGuidWithAccess,
            TestFactory.CheckListGuidRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid,
            DateTime.UtcNow,
            TestFactory.PriorityGuid,
            TestFactory.SortingGuid,
            TestFactory.TypeGuid);
        return guidAndRowVersion;
    }

    private async Task<(
        GuidAndRowVersion punchItemGuidAndRowVersion,
        GuidAndRowVersion linkGuidAndRowVersion)> CreatePunchItemLinkInRestrictedCheckListAsync()
    {
        var punchItemGuidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        var linkGuidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            "T",
            "U");

        return (punchItemGuidAndRowVersion, linkGuidAndRowVersion);
    }

    private async Task<(GuidAndRowVersion punchItemGuidAndRowVersion, GuidAndRowVersion linkGuidAndRowVersion)>
        UploadNewPunchItemAttachmentInRestrictedCheckListAsync(string fileName)
    {
        var punchItemGuidAndRowVersion = await CreatePunchItemInRestrictedCheckListAsync();

        var attachmentGuidAndRowVersion = await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            new TestFile("blah", fileName));

        return (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion);
    }
}
