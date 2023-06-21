using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Punches;

[TestClass]
public class PunchesControllerTests : TestBase
{
    private Guid _punchGuidUnderTest;
    private List<PunchDto> _initialPunchesInProject;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _punchGuidUnderTest = TestFactory.Instance.SeededData[KnownPlantData.PlantA].PunchAGuid;
        _initialPunchesInProject = await PunchesControllerTestsHelper
            .GetAllPunchesInProjectAsync(UserType.Reader, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);
    }

    [TestMethod]
    public async Task CreatePunch_AsWriter_ShouldCreatePunch()
    {
        // Arrange
        var itemNo = Guid.NewGuid().ToString();

        // Act
        var guidAndRowVersion = await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            itemNo,
            TestFactory.ProjectGuidWithAccess);

        // Assert
        AssertValidGuidAndRowVersion(guidAndRowVersion);
        var newPunch = await PunchesControllerTestsHelper
            .GetPunchAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsNotNull(newPunch);
        Assert.AreEqual(itemNo, newPunch.ItemNo);
        AssertCreatedBy(UserType.Writer, newPunch.CreatedBy);

        var allPunches = await PunchesControllerTestsHelper
            .GetAllPunchesInProjectAsync(UserType.Writer, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);
        Assert.AreEqual(_initialPunchesInProject.Count+1, allPunches.Count);
    }

    [TestMethod]
    public async Task GetPunch_AsReader_ShouldGetPunch()
    {
        // Act
        var punch = await PunchesControllerTestsHelper
            .GetPunchAsync(UserType.Reader, TestFactory.PlantWithAccess, _punchGuidUnderTest);

        // Assert
        Assert.AreEqual(_punchGuidUnderTest, punch.Guid);
        Assert.IsNotNull(punch.RowVersion);
    }

    [TestMethod]
    public async Task GetAllPunches_AsReader_ShouldGetAllPunches()
    {
        // Act
        var punches = await PunchesControllerTestsHelper
            .GetAllPunchesInProjectAsync(UserType.Reader, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);

        // Assert
        Assert.IsTrue(punches.Count > 0);
        Assert.IsTrue(punches.All(p => !p.ItemNo.IsEmpty()));
        Assert.IsTrue(punches.All(p => !p.RowVersion.IsEmpty()));
    }

    [TestMethod]
    public async Task UpdatePunch_AsWriter_ShouldUpdatePunchAndRowVersion()
    {
        // Arrange
        var newDescription = Guid.NewGuid().ToString();
        var punch = await PunchesControllerTestsHelper.GetPunchAsync(UserType.Writer, TestFactory.PlantWithAccess, _punchGuidUnderTest);
        var initialRowVersion = punch.RowVersion;

        // Act
        var newRowVersion = await PunchesControllerTestsHelper.UpdatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punch.Guid,
            newDescription,
            initialRowVersion);

        // Assert
        AssertRowVersionChange(initialRowVersion, newRowVersion);
        punch = await PunchesControllerTestsHelper.GetPunchAsync(UserType.Writer, TestFactory.PlantWithAccess, _punchGuidUnderTest);
        Assert.AreEqual(newDescription, punch.Description);
        Assert.AreEqual(newRowVersion, punch.RowVersion);
    }

    [TestMethod]
    public async Task VoidPunch_AsWriter_ShouldVoidPunch()
    {
        // Arrange
        var guidAndRowVersion = await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess);
        var initialRowVersion = guidAndRowVersion.RowVersion;

        // Act
        var newRowVersion = await PunchesControllerTestsHelper.VoidPunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guidAndRowVersion.Guid,
            guidAndRowVersion.RowVersion);

        // Assert
        AssertRowVersionChange(initialRowVersion, newRowVersion);
        var punch = await PunchesControllerTestsHelper.GetPunchAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsTrue(punch.IsVoided);
        Assert.AreEqual(newRowVersion, punch.RowVersion);
    }

    [TestMethod]
    public async Task DeletePunch_AsWriter_ShouldDeletePunch()
    {
        // Arrange
        var guidAndRowVersion = await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess);
        var newRowVersion = await PunchesControllerTestsHelper.VoidPunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guidAndRowVersion.Guid,
            guidAndRowVersion.RowVersion);
        var punch = await PunchesControllerTestsHelper.GetPunchAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsNotNull(punch);

        // Act
        await PunchesControllerTestsHelper.DeletePunchAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guidAndRowVersion.Guid,
            newRowVersion);

        // Assert
        await PunchesControllerTestsHelper.GetPunchAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid, HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task CreatePunchLink_AsWriter_ShouldCreatePunchLink()
    {
        // Arrange and Act
        var (_, linkGuidAndRowVersion)
            = await CreatePunchLinkAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        AssertValidGuidAndRowVersion(linkGuidAndRowVersion);
    }

    [TestMethod]
    public async Task GetPunchLinksAsync_AsReader_ShouldGetPunchLinks()
    {
        // Arrange and Act
        var title = Guid.NewGuid().ToString();
        var url = Guid.NewGuid().ToString();
        var (punchGuidAndRowVersion, linkGuidAndRowVersion) = await CreatePunchLinkAsync(title, url);

        // Act
        var links = await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);

        // Assert
        AssertFirstAndOnlyLink(
            punchGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.RowVersion,
            title,
            url,
            links);
    }

    [TestMethod]
    public async Task UpdatePunchLink_AsWriter_ShouldUpdatePunchLinkAndRowVersion()
    {
        // Arrange
        var newTitle = Guid.NewGuid().ToString();
        var newUrl = Guid.NewGuid().ToString();
        var (punchGuidAndRowVersion, linkGuidAndRowVersion) =
            await CreatePunchLinkAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Act
        var newRowVersion = await PunchesControllerTestsHelper.UpdatePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            newTitle,
            newUrl,
            linkGuidAndRowVersion.RowVersion);

        // Assert
        var links = await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);

        AssertRowVersionChange(linkGuidAndRowVersion.RowVersion, newRowVersion);
        AssertFirstAndOnlyLink(
            punchGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            newRowVersion,
            newTitle, 
            newUrl,
            links);
    }

    [TestMethod]
    public async Task DeletePunchLink_AsWriter_ShouldDeletePunchLink()
    {
        // Arrange
        var (punchGuidAndRowVersion, linkGuidAndRowVersion)
            = await CreatePunchLinkAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        var links = await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);
        Assert.AreEqual(1, links.Count);

        // Act
        await PunchesControllerTestsHelper.DeletePunchLinkAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.RowVersion);

        // Assert
        links = await PunchesControllerTestsHelper.GetPunchLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);
        Assert.AreEqual(0, links.Count);
    }

    [TestMethod]
    public async Task CreatePunchComment_AsWriter_ShouldCreatePunchComment()
    {
        // Arrange and Act
        var (_, commentGuidAndRowVersion)
            = await CreatePunchCommentAsync(Guid.NewGuid().ToString());

        // Assert
        AssertValidGuidAndRowVersion(commentGuidAndRowVersion);
    }

    [TestMethod]
    public async Task GetPunchCommentsAsync_AsReader_ShouldGetPunchComments()
    {
        // Arrange and Act
        var text = Guid.NewGuid().ToString();
        var (punchGuidAndRowVersion, commentGuidAndRowVersion) = await CreatePunchCommentAsync(text);

        // Act
        var comments = await PunchesControllerTestsHelper.GetPunchCommentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);

        // Assert
        AssertFirstAndOnlyComment(
            punchGuidAndRowVersion.Guid,
            commentGuidAndRowVersion.Guid,
            text,
            comments);
    }

    [TestMethod]
    public async Task UploadPunchAttachment_AsWriter_ShouldUploadPunchAttachment()
    {
        // Arrange and Act
        var (_, attachmentGuidAndRowVersion)
            = await UploadNewPunchAttachmentAsync(Guid.NewGuid().ToString());

        // Assert
        AssertValidGuidAndRowVersion(attachmentGuidAndRowVersion);
    }

    [TestMethod]
    public async Task GetPunchAttachmentsAsync_AsReader_ShouldGetPunchAttachments()
    {
        // Arrange and Act
        var fileName = Guid.NewGuid().ToString();
        var (punchGuidAndRowVersion, attachmentGuidAndRowVersion) = await UploadNewPunchAttachmentAsync(fileName);

        // Act
        var attachments = await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);

        // Assert
        AssertFirstAndOnlyAttachment(
            punchGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.RowVersion,
            fileName,
            attachments);
    }

    [TestMethod]
    public async Task GetPunchAttachmentDownloadUrl_AsReader_ShouldGetUrl()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchGuidAndRowVersion, attachmentGuidAndRowVersion) = await UploadNewPunchAttachmentAsync(fileName);

        var attachments = await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);
        var uri = new Uri("http://blah.blah.com");
        var fullBlobPath = attachments.ElementAt(0).FullBlobPath;
        TestFactory.Instance.BlobStorageMock
            .Setup(a => a.GetDownloadSasUri(
                It.IsAny<string>(),
                fullBlobPath,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .Returns(uri);


        // Act
        var attachmentUrl = await PunchesControllerTestsHelper.GetPunchAttachmentDownloadUrlAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid);

        // Assert
        Assert.AreEqual(uri.AbsoluteUri, attachmentUrl);
    }

    [TestMethod]
    public async Task OverwriteExistingPunchAttachment_AsWriter_ShouldUpdatePunchAttachmentAndRowVersion()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchGuidAndRowVersion, attachmentGuidAndRowVersion) =
            await UploadNewPunchAttachmentAsync(fileName);

        // Act
        var newAttachmentRowVersion = await PunchesControllerTestsHelper.OverwriteExistingPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid,
            new TestFile("blah updated", fileName),
            attachmentGuidAndRowVersion.RowVersion);

        // Assert
        AssertRowVersionChange(attachmentGuidAndRowVersion.RowVersion, newAttachmentRowVersion);

        var attachments = await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);

        AssertFirstAndOnlyAttachment(
            punchGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            newAttachmentRowVersion,
            fileName,
            attachments);
    }

    [TestMethod]
    public async Task DeletePunchAttachment_AsWriter_ShouldDeletePunchAttachment()
    {
        // Arrange
        var (punchGuidAndRowVersion, attachmentGuidAndRowVersion)
            = await UploadNewPunchAttachmentAsync(Guid.NewGuid().ToString());
        var attachments = await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);
        Assert.AreEqual(1, attachments.Count);

        // Act
        await PunchesControllerTestsHelper.DeletePunchAttachmentAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.RowVersion);

        // Assert
        attachments = await PunchesControllerTestsHelper.GetPunchAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid);
        Assert.AreEqual(0, attachments.Count);
    }

    private async Task<(
        GuidAndRowVersion punchGuidAndRowVersion, 
        GuidAndRowVersion linkGuidAndRowVersion)>CreatePunchLinkAsync(string title, string url)
    {
        var punchGuidAndRowVersion = await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess);

        var linkGuidAndRowVersion = await PunchesControllerTestsHelper.CreatePunchLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid,
            title,
            url);

        return (punchGuidAndRowVersion, linkGuidAndRowVersion);
    }

    private async Task<(GuidAndRowVersion punchGuidAndRowVersion, GuidAndRowVersion commentGuidAndRowVersion)>
        CreatePunchCommentAsync(string text)
    {
        var punchGuidAndRowVersion = await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess);

        var commentGuidAndRowVersion = await PunchesControllerTestsHelper.CreatePunchCommentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid,
            text);

        return (punchGuidAndRowVersion, commentGuidAndRowVersion);
    }

    private async Task<(GuidAndRowVersion punchGuidAndRowVersion, GuidAndRowVersion linkGuidAndRowVersion)>
        UploadNewPunchAttachmentAsync(string fileName)
    {
        var punchGuidAndRowVersion = await PunchesControllerTestsHelper.CreatePunchAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            Guid.NewGuid().ToString(),
            TestFactory.ProjectGuidWithAccess);

        var attachmentGuidAndRowVersion = await PunchesControllerTestsHelper.UploadNewPunchAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchGuidAndRowVersion.Guid,
            new TestFile("blah", fileName));

        return (punchGuidAndRowVersion, attachmentGuidAndRowVersion);
    }

    private static void AssertFirstAndOnlyLink(
        Guid punchGuid,
        Guid linkGuid,
        string linkRowVersion,
        string title,
        string url,
        List<LinkDto> links)
    {
        Assert.IsNotNull(links);
        Assert.AreEqual(1, links.Count);
        var link = links[0];
        Assert.AreEqual(punchGuid, link.SourceGuid);
        Assert.AreEqual(linkGuid, link.Guid);
        Assert.AreEqual(linkRowVersion, link.RowVersion);
        Assert.AreEqual(title, link.Title);
        Assert.AreEqual(url, link.Url);
    }

    private static void AssertFirstAndOnlyComment(
        Guid punchGuid,
        Guid commentGuid,
        string text,
        List<CommentDto> comments)
    {
        Assert.IsNotNull(comments);
        Assert.AreEqual(1, comments.Count);
        var comment = comments[0];
        Assert.AreEqual(punchGuid, comment.SourceGuid);
        Assert.AreEqual(commentGuid, comment.Guid);
        Assert.AreEqual(text, comment.Text);
        Assert.IsNotNull(comment.CreatedBy);
        Assert.IsNotNull(comment.CreatedAtUtc);
    }

    private static void AssertFirstAndOnlyAttachment(
        Guid punchGuid,
        Guid attachmentGuid,
        string attachmentRowVersion,
        string fileName,
        List<AttachmentDto> attachments)
    {
        Assert.IsNotNull(attachments);
        Assert.AreEqual(1, attachments.Count);
        var attachment = attachments[0];
        Assert.AreEqual(punchGuid, attachment.SourceGuid);
        Assert.AreEqual(attachmentGuid, attachment.Guid);
        Assert.AreEqual(attachmentRowVersion, attachment.RowVersion);
        Assert.AreEqual(fileName, attachment.FileName);
    }
}
