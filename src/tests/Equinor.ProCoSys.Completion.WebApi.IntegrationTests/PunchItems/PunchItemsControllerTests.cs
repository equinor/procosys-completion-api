using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

[TestClass]
public class PunchItemsControllerTests : TestBase
{
    private Guid _punchItemGuidUnderTest;
    private List<PunchItemDto> _initialPunchItemsInProject;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _punchItemGuidUnderTest = TestFactory.Instance.SeededData[TestFactory.PlantWithAccess].PunchItemA.Guid;
        _initialPunchItemsInProject = await PunchItemsControllerTestsHelper
            .GetAllPunchItemsInProjectAsync(UserType.Reader, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);
        TestFactory.Instance.SetupBlobStorageMock(new Uri("http://blah.blah.com"));
    }

    [TestMethod]
    public async Task CreatePunchItem_AsWriter_ShouldCreatePunchItem()
    {
        // Arrange
        var description = Guid.NewGuid().ToString();
        var category = "PB";

        // Act
        var guidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            category,
            description,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid,
            DateTime.UtcNow,
            TestFactory.PriorityGuid,
            TestFactory.SortingGuid,
            TestFactory.TypeGuid);

        // Assert
        AssertValidGuidAndRowVersion(guidAndRowVersion);
        var newPunchItem = await PunchItemsControllerTestsHelper
            .GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsNotNull(newPunchItem);
        Assert.AreEqual(category, newPunchItem.Category);
        Assert.AreEqual(description, newPunchItem.Description);
        Assert.IsTrue(newPunchItem.ItemNo >= PunchItem.IdentitySeed);
        AssertCreatedBy(UserType.Writer, newPunchItem.CreatedBy);
        Assert.IsTrue(PunchItem.ItemNoStartsAtt < newPunchItem.ItemNo);
        Assert.AreEqual(TestFactory.ClearingByOrgGuid, newPunchItem.ClearingByOrg.Guid);
        Assert.AreEqual(TestFactory.RaisedByOrgGuid, newPunchItem.RaisedByOrg!.Guid);
        Assert.AreEqual(TestFactory.PriorityGuid, newPunchItem.Priority!.Guid);
        Assert.AreEqual(TestFactory.SortingGuid, newPunchItem.Sorting!.Guid);
        Assert.AreEqual(TestFactory.TypeGuid, newPunchItem.Type!.Guid);

        var allPunchItems = await PunchItemsControllerTestsHelper
            .GetAllPunchItemsInProjectAsync(UserType.Writer, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);
        Assert.AreEqual(_initialPunchItemsInProject.Count+1, allPunchItems.Count);
    }

    [TestMethod]
    public async Task GetPunchItem_AsReader_ShouldGetPunchItem()
    {
        // Act
        var punchItem = await PunchItemsControllerTestsHelper
            .GetPunchItemAsync(UserType.Reader, TestFactory.PlantWithAccess, _punchItemGuidUnderTest);

        // Assert
        Assert.AreEqual(_punchItemGuidUnderTest, punchItem.Guid);
        Assert.IsNotNull(punchItem.RowVersion);
        Assert.IsNotNull(punchItem.CreatedBy);
        Assert.IsNotNull(punchItem.RaisedByOrg);
        Assert.IsNotNull(punchItem.ClearingByOrg);
        Assert.IsNotNull(punchItem.Priority);
        Assert.IsNotNull(punchItem.Sorting);
        Assert.IsNotNull(punchItem.Type);
    }

    [TestMethod]
    public async Task GetAllPunchItemsInProject_AsReader_ShouldGetPunchItems()
    {
        // Act
        var punchItems = await PunchItemsControllerTestsHelper
            .GetAllPunchItemsInProjectAsync(UserType.Reader, TestFactory.PlantWithAccess, TestFactory.ProjectGuidWithAccess);

        // Assert (can't assert the exact number since other tests creates items in in-memory db)
        Assert.IsTrue(punchItems.Count > 0);
    }

    [TestMethod]
    public async Task UpdatePunchItem_WithNonNullValues_AsWriter_ShouldUpdateRowVersion_AndPatchPunchItemWithNonNullValues()
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
        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.AreEqual(TestFactory.ClearingByOrgGuid, punchItem.ClearingByOrg.Guid);
        Assert.AreEqual(TestFactory.RaisedByOrgGuid, punchItem.RaisedByOrg!.Guid);
        Assert.IsNull(punchItem.Priority);
        Assert.IsNull(punchItem.Sorting);
        Assert.IsNull(punchItem.Type);
        var initialRowVersion = punchItem.RowVersion;
        var patchDocument = new JsonPatchDocument();
        var newDescription = Guid.NewGuid().ToString();
        patchDocument.Replace("Description", newDescription);
        patchDocument.Replace("PriorityGuid", TestFactory.PriorityGuid);
        patchDocument.Replace("SortingGuid", TestFactory.SortingGuid);
        patchDocument.Replace("TypeGuid", TestFactory.TypeGuid);
        patchDocument.Replace("ActionByPersonOid", TestFactory.Instance.GetTestProfile(UserType.Writer).Oid);
        var newDueTimeUtc = DateTime.UtcNow.AddDays(7);
        patchDocument.Replace("DueTimeUtc", newDueTimeUtc);
        var newEstimate = 8;
        patchDocument.Replace("Estimate", newEstimate);
        patchDocument.Replace("OriginalWorkOrderGuid", TestFactory.OriginalWorkOrderGuid);
        patchDocument.Replace("WorkOrderGuid", TestFactory.WorkOrderGuid);
        patchDocument.Replace("SWCRGuid", TestFactory.SWCRGuid);
        patchDocument.Replace("DocumentGuid", TestFactory.DocumentGuid);
        var newExternalItemNo = "123a";
        patchDocument.Replace("ExternalItemNo", newExternalItemNo);
        const bool NewMaterialRequired = true;
        patchDocument.Replace("MaterialRequired", NewMaterialRequired);
        var newMaterialETAUtc = DateTime.UtcNow.AddDays(7);
        patchDocument.Replace("MaterialETAUtc", newMaterialETAUtc);
        var newMaterialExternalNo = "A-1";
        patchDocument.Replace("MaterialExternalNo", newMaterialExternalNo);

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItem.Guid,
            patchDocument,
            initialRowVersion);

        // Assert
        AssertRowVersionChange(initialRowVersion, newRowVersion);
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.AreEqual(newRowVersion, punchItem.RowVersion);
        Assert.AreEqual(newDescription, punchItem.Description);
        Assert.AreEqual(TestFactory.ClearingByOrgGuid, punchItem.ClearingByOrg.Guid, "Value not patched and should be kept");
        Assert.AreEqual(TestFactory.RaisedByOrgGuid, punchItem.RaisedByOrg!.Guid, "Value not patched and should be kept");
        Assert.AreEqual(TestFactory.PriorityGuid, punchItem.Priority!.Guid);
        Assert.AreEqual(TestFactory.SortingGuid, punchItem.Sorting!.Guid);
        Assert.AreEqual(TestFactory.TypeGuid, punchItem.Type!.Guid);
        Assert.AreEqual(TestFactory.Instance.GetTestProfile(UserType.Writer).Guid, punchItem.ActionBy!.Guid);
        Assert.AreEqual(newDueTimeUtc, punchItem.DueTimeUtc);
        Assert.AreEqual(newEstimate, punchItem.Estimate);
        Assert.AreEqual(TestFactory.OriginalWorkOrderGuid, punchItem.OriginalWorkOrder!.Guid);
        Assert.AreEqual(TestFactory.WorkOrderGuid, punchItem.WorkOrder!.Guid);
        Assert.AreEqual(TestFactory.SWCRGuid, punchItem.SWCR!.Guid);
        Assert.AreEqual(TestFactory.DocumentGuid, punchItem.Document!.Guid);
        Assert.AreEqual(newExternalItemNo, punchItem.ExternalItemNo);
        Assert.AreEqual(NewMaterialRequired, punchItem.MaterialRequired);
        Assert.AreEqual(newMaterialETAUtc, punchItem.MaterialETAUtc);
        Assert.AreEqual(newMaterialExternalNo, punchItem.MaterialExternalNo);
    }

    [TestMethod]
    public async Task UpdatePunchItem_WithNullValues_AsWriter_ShouldUpdateRowVersion_AndPatchPunchItemWithNullValues()
    {
        // Arrange
        var guidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            Guid.NewGuid().ToString(),
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid,
            priorityGuid: TestFactory.PriorityGuid,
            sortingGuid: TestFactory.SortingGuid,
            typeGuid: TestFactory.TypeGuid,
            originalWorkOrderGuid: TestFactory.OriginalWorkOrderGuid,
            workOrderGuid:TestFactory.WorkOrderGuid,
            swcrGuid: TestFactory.SWCRGuid,
            documentGuid: TestFactory.DocumentGuid
            );
        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.AreEqual(TestFactory.ClearingByOrgGuid, punchItem.ClearingByOrg.Guid);
        Assert.AreEqual(TestFactory.RaisedByOrgGuid, punchItem.RaisedByOrg!.Guid);
        Assert.AreEqual(TestFactory.PriorityGuid, punchItem.Priority!.Guid);
        Assert.AreEqual(TestFactory.SortingGuid, punchItem.Sorting!.Guid);
        Assert.AreEqual(TestFactory.TypeGuid, punchItem.Type!.Guid);
        Assert.AreEqual(TestFactory.OriginalWorkOrderGuid, punchItem.OriginalWorkOrder!.Guid);
        Assert.AreEqual(TestFactory.WorkOrderGuid, punchItem.WorkOrder!.Guid);
        Assert.AreEqual(TestFactory.SWCRGuid, punchItem.SWCR!.Guid);
        Assert.AreEqual(TestFactory.DocumentGuid, punchItem.Document!.Guid);

        var initialRowVersion = punchItem.RowVersion;
        var patchDocument = new JsonPatchDocument();
        patchDocument.Replace("PriorityGuid", null);
        patchDocument.Replace("SortingGuid", null);
        patchDocument.Replace("TypeGuid", null);
        patchDocument.Replace("OriginalWorkOrderGuid", null);
        patchDocument.Replace("WorkOrderGuid", null);
        patchDocument.Replace("SWCRGuid", null);
        patchDocument.Replace("DocumentGuid", null);

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItem.Guid,
            patchDocument,
            initialRowVersion);

        // Assert
        AssertRowVersionChange(initialRowVersion, newRowVersion);
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.AreEqual(newRowVersion, punchItem.RowVersion);
        Assert.AreEqual(TestFactory.ClearingByOrgGuid, punchItem.ClearingByOrg.Guid, "Value not patched and should be kept");
        Assert.AreEqual(TestFactory.RaisedByOrgGuid, punchItem.RaisedByOrg!.Guid, "Value not patched and should be kept");
        Assert.IsNull(punchItem.Priority);
        Assert.IsNull(punchItem.Sorting);
        Assert.IsNull(punchItem.Type);
        Assert.IsNull(punchItem.OriginalWorkOrder);
        Assert.IsNull(punchItem.WorkOrder);
        Assert.IsNull(punchItem.SWCR);
        Assert.IsNull(punchItem.Document);
    }

    [TestMethod]
    public async Task UpdatePunchItem_WithoutAnyValues_AsWriter_ShouldLeaveBothRowVersionAndPunchItemUnchanged()
    {
        // Arrange
        var description = Guid.NewGuid().ToString();
        var guidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            description,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid,
            priorityGuid: TestFactory.PriorityGuid,
            sortingGuid: TestFactory.SortingGuid,
            typeGuid: TestFactory.TypeGuid);
        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.AreEqual(description, punchItem.Description);
        Assert.AreEqual(TestFactory.ClearingByOrgGuid, punchItem.ClearingByOrg.Guid);
        Assert.AreEqual(TestFactory.RaisedByOrgGuid, punchItem.RaisedByOrg!.Guid);
        Assert.AreEqual(TestFactory.PriorityGuid, punchItem.Priority!.Guid);
        Assert.AreEqual(TestFactory.SortingGuid, punchItem.Sorting!.Guid);
        Assert.AreEqual(TestFactory.TypeGuid, punchItem.Type!.Guid);
        var initialRowVersion = punchItem.RowVersion;

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.UpdatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItem.Guid,
            new JsonPatchDocument(),
            initialRowVersion);

        // Assert
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.AreEqual(initialRowVersion, newRowVersion);
        Assert.AreEqual(newRowVersion, punchItem.RowVersion);
        Assert.AreEqual(description, punchItem.Description, "Value not patched and should be kept");
        Assert.AreEqual(TestFactory.ClearingByOrgGuid, punchItem.ClearingByOrg.Guid, "Value not patched and should be kept");
        Assert.AreEqual(TestFactory.RaisedByOrgGuid, punchItem.RaisedByOrg!.Guid, "Value not patched and should be kept");
        Assert.AreEqual(TestFactory.PriorityGuid, punchItem.Priority!.Guid, "Value not patched and should be kept");
        Assert.AreEqual(TestFactory.SortingGuid, punchItem.Sorting!.Guid, "Value not patched and should be kept");
        Assert.AreEqual(TestFactory.TypeGuid, punchItem.Type!.Guid, "Value not patched and should be kept");
    }

    [TestMethod]
    public async Task UpdatePunchItemCategory_AsWriter_ShouldUpdatePunchItemCategory()
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
        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsNotNull(punchItem);
        Assert.AreEqual("PA", punchItem.Category);

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.UpdatePunchItemCategoryAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guidAndRowVersion.Guid,
            "PB",
            guidAndRowVersion.RowVersion);

        // Assert
        AssertRowVersionChange(guidAndRowVersion.RowVersion, newRowVersion);
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsNotNull(punchItem);
        Assert.AreEqual("PB", punchItem.Category);
    }

    [TestMethod]
    public async Task DeletePunchItem_AsWriter_ShouldDeletePunchItem()
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
        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsNotNull(punchItem);

        // Act
        await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guidAndRowVersion.Guid,
            guidAndRowVersion.RowVersion);

        // Assert
        await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid, HttpStatusCode.NotFound);
    }
    
    [TestMethod]
    public async Task DeletePunchItemWithRelatedEntities_ShouldDeletePunchItemAndAllRelatedEntities()
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
        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsNotNull(punchItem);

        await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItem.Guid,
            "tull og tøys",
            [],
            []);
        
        await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItem.Guid,
            "Some title",
            "http://www.google.com");

       var attachmentGuidAndRowVersion = await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItem.Guid,
            new TestFile("asdf", "fun file name")
        );
        var blobPath = $"ProjectNameA/PunchItem/{attachmentGuidAndRowVersion.Guid}/fun file name";
        // Act
        await PunchItemsControllerTestsHelper.DeletePunchItemAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guidAndRowVersion.Guid,
            guidAndRowVersion.RowVersion);

        // Assert
        await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid, HttpStatusCode.NotFound);
        await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid, HttpStatusCode.NotFound);
        await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid, HttpStatusCode.NotFound);
        await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid, HttpStatusCode.NotFound);
        
        //Checking that Blobstorage got a call to delete, via published message
        await TestFactory.Instance.BlobStorageMock.Received(1).DeleteAsync("procosys-attachments", blobPath, Arg.Any<CancellationToken>());
    
    }

    [TestMethod]
    public async Task CreatePunchItemLink_AsWriter_ShouldCreatePunchItemLink()
    {
        // Arrange and Act
        var (_, linkGuidAndRowVersion)
            = await CreatePunchItemLinkAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        AssertValidGuidAndRowVersion(linkGuidAndRowVersion);
    }

    [TestMethod]
    public async Task GetPunchItemLinksAsync_AsReader_ShouldGetPunchItemLinks()
    {
        // Arrange
        var title = Guid.NewGuid().ToString();
        var url = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, linkGuidAndRowVersion) = await CreatePunchItemLinkAsync(title, url);

        // Act
        var links = await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);

        // Assert
        AssertFirstAndOnlyLink(
            punchItemGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.RowVersion,
            title,
            url,
            links);
    }

    [TestMethod]
    public async Task UpdatePunchItemLink_AsWriter_ShouldUpdatePunchItemLinkAndRowVersion()
    {
        // Arrange
        var newTitle = Guid.NewGuid().ToString();
        var newUrl = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, linkGuidAndRowVersion) =
            await CreatePunchItemLinkAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.UpdatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            newTitle,
            newUrl,
            linkGuidAndRowVersion.RowVersion);

        // Assert
        var links = await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);

        AssertRowVersionChange(linkGuidAndRowVersion.RowVersion, newRowVersion);
        AssertFirstAndOnlyLink(
            punchItemGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            newRowVersion,
            newTitle, 
            newUrl,
            links);
    }

    [TestMethod]
    public async Task DeletePunchItemLink_AsWriter_ShouldDeletePunchItemLink()
    {
        // Arrange
        var (punchItemGuidAndRowVersion, linkGuidAndRowVersion)
            = await CreatePunchItemLinkAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        var links = await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);
        Assert.AreEqual(1, links.Count);

        // Act
        await PunchItemsControllerTestsHelper.DeletePunchItemLinkAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.Guid,
            linkGuidAndRowVersion.RowVersion);

        // Assert
        links = await PunchItemsControllerTestsHelper.GetPunchItemLinksAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);
        Assert.AreEqual(0, links.Count);
    }

    [TestMethod]
    public async Task CreatePunchItemComment_AsWriter_ShouldCreatePunchItemComment()
    {
        // Arrange and Act
        var (_, commentGuidAndRowVersion)
            = await CreatePunchItemCommentAsync(Guid.NewGuid().ToString(), [], []);

        // Assert
        AssertValidGuidAndRowVersion(commentGuidAndRowVersion);
    }

    [TestMethod]
    public async Task GetPunchItemCommentsAsync_AsReader_ShouldGetPunchItemCommentsWithLabelsAndMentions()
    {
        // Arrange
        var text = Guid.NewGuid().ToString();
        var labels = new List<string> { "A", "B" };
        var mentions = new List<Guid>
        {
            TestFactory.Instance.GetTestProfile(UserType.RestrictedWriter).Guid,
            TestFactory.Instance.GetTestProfile(UserType.Reader).Guid
        };

        var (punchItemGuidAndRowVersion, commentGuidAndRowVersion)
            = await CreatePunchItemCommentAsync(text, labels, mentions);

        // Act
        var comments = await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);

        // Assert
        AssertFirstAndOnlyComment(
            comments,
            punchItemGuidAndRowVersion.Guid,
            commentGuidAndRowVersion.Guid,
            text,
            labels,
            mentions);
    }

    [TestMethod]
    public async Task GetPunchItemHistoryAsync_AsReader_ShouldGetPunchItemHistoryWithProperties()
    {
        // Act
        var history = await PunchItemsControllerTestsHelper.GetPunchItemHistoryAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            _punchItemGuidUnderTest
        );

        Assert.AreEqual(_punchItemGuidUnderTest, history.First().EventForGuid);
    }

    [TestMethod]
    public async Task UploadPunchItemAttachment_AsWriter_ShouldUploadPunchAttachment()
    {
        // Arrange and Act
        var (_, attachmentGuidAndRowVersion)
            = await UploadNewPunchItemAttachmentOnVerifiedPunchAsync(Guid.NewGuid().ToString());

        // Assert
        AssertValidGuidAndRowVersion(attachmentGuidAndRowVersion);
    }

    [TestMethod]
    public async Task GetPunchItemAttachmentsAsync_AsReader_ShouldGetPunchItemAttachments()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion) = await UploadNewPunchItemAttachmentOnVerifiedPunchAsync(fileName);

        // Act
        var attachments = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);

        // Assert
        AssertFirstAndOnlyAttachment(
            punchItemGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.RowVersion,
            fileName,
            attachments);
    }

    [TestMethod]
    public async Task GetPunchItemAttachmentDownloadUrl_AsReader_ShouldGetUrl()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion) = await UploadNewPunchItemAttachmentOnVerifiedPunchAsync(fileName);

        var attachments = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);
        var uri = new Uri("http://blah.blah.com");
        var fullBlobPath = attachments.ElementAt(0).FullBlobPath;
        TestFactory.Instance.BlobStorageMock
            .GetDownloadSasUri(
                Arg.Any<string>(),
                fullBlobPath,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>())
            .Returns(uri);

        // Act
        var attachmentUrl = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentDownloadUrlAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid);

        // Assert
        Assert.AreEqual(uri.AbsoluteUri, attachmentUrl);
    }

    [TestMethod]
    public async Task UpdatePunchItemAttachment_AsWriter_ShouldUpdateAttachment()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion) = await UploadNewPunchItemAttachmentOnVerifiedPunchAsync(fileName);

        var description = Guid.NewGuid().ToString();
        var labelA = KnownData.LabelA;
        var labelB = KnownData.LabelB;

        // Act
        var newAttachmentRowVersion = await PunchItemsControllerTestsHelper.UpdatePunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            description,
            new List<string>{ labelA, labelB },
            attachmentGuidAndRowVersion.RowVersion);

        // Assert
        AssertRowVersionChange(attachmentGuidAndRowVersion.RowVersion, newAttachmentRowVersion);

        var attachmentDtos = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);
        var attachmentDto = attachmentDtos.Single(dto => dto.Guid == attachmentGuidAndRowVersion.Guid);
        Assert.AreEqual(description, attachmentDto.Description);
        Assert.AreEqual(2, attachmentDto.Labels.Count);
        Assert.AreEqual(labelA, attachmentDto.Labels.ElementAt(0));
        Assert.AreEqual(labelB, attachmentDto.Labels.ElementAt(1));
    }

    [TestMethod]
    public async Task OverwriteExistingPunchItemAttachment_AsWriter_ShouldUpdatePunchItemAttachmentAndRowVersion()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion) =
            await UploadNewPunchItemAttachmentOnVerifiedPunchAsync(fileName);

        // Act
        var newAttachmentRowVersion = await PunchItemsControllerTestsHelper.OverwriteExistingPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            new TestFile("blah updated", fileName),
            attachmentGuidAndRowVersion.RowVersion);

        // Assert
        AssertRowVersionChange(attachmentGuidAndRowVersion.RowVersion, newAttachmentRowVersion);

        var attachments = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);

        AssertFirstAndOnlyAttachment(
            punchItemGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            newAttachmentRowVersion,
            fileName,
            attachments);
    }

    [TestMethod]
    public async Task DeletePunchItemAttachment_AsWriter_ShouldDeletePunchItemAttachment()
    {
        // Arrange
        var (punchItemGuidAndRowVersion, attachmentGuidAndRowVersion)
            = await UploadNewPunchItemAttachmentOnVerifiedPunchAsync(Guid.NewGuid().ToString());
        var attachments = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);
        Assert.AreEqual(1, attachments.Count);
        var blobPath = attachments.First().FullBlobPath;

        // Act
        await PunchItemsControllerTestsHelper.DeletePunchItemAttachmentAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.Guid,
            attachmentGuidAndRowVersion.RowVersion);

        // Assert
        attachments = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid);
        Assert.AreEqual(0, attachments.Count);
        
        //Checking that Blobstorage got a call to delete, via published message
        await TestFactory.Instance.BlobStorageMock.Received(1).DeleteAsync("procosys-attachments", blobPath, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ClearPunchItem_AsWriter_ShouldClearPunchItem()
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
        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsNotNull(punchItem);
        Assert.IsTrue(punchItem.IsReadyToBeCleared);

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guidAndRowVersion.Guid,
            guidAndRowVersion.RowVersion);

        // Assert
        AssertRowVersionChange(guidAndRowVersion.RowVersion, newRowVersion);
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guidAndRowVersion.Guid);
        Assert.IsFalse(punchItem.IsReadyToBeCleared);
    }

    [TestMethod]
    public async Task UnclearPunchItem_AsWriter_ShouldUnclearPunchItem()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guid);
        Assert.IsNotNull(punchItem);
        Assert.IsTrue(punchItem.IsReadyToBeUncleared);

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.UnclearPunchItemAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guid,
            rowVersionAfterClear);

        // Assert
        AssertRowVersionChange(rowVersionAfterClear, newRowVersion);
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guid);
        Assert.IsFalse(punchItem.IsReadyToBeUncleared);
    }

    [TestMethod]
    public async Task RejectPunchItem_AsWriter_ShouldRejectPunchItem()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guid);
        Assert.IsNotNull(punchItem);
        Assert.IsTrue(punchItem.IsReadyToBeRejected);

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guid,
            Guid.NewGuid().ToString(),
            [],
            rowVersionAfterClear);

        // Assert
        AssertRowVersionChange(rowVersionAfterClear, newRowVersion);
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guid);
        Assert.IsFalse(punchItem.IsReadyToBeRejected);
    }

    [TestMethod]
    public async Task RejectPunchItem_AsWriter_ShouldAddRejectedCommentWithMentionsToComments()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        var mentions = new List<Guid>
        {
            TestFactory.Instance.GetTestProfile(UserType.RestrictedWriter).Guid,
            TestFactory.Instance.GetTestProfile(UserType.Reader).Guid
        };
        var comment = $"Must approve {Guid.NewGuid()}";

        // Act
        await PunchItemsControllerTestsHelper.RejectPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guid,
            comment,
            mentions,
            rowVersionAfterClear);

        // Assert
        var comments = await PunchItemsControllerTestsHelper.GetPunchItemCommentsAsync(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            guid);

        // Assert
        Assert.IsNotNull(comments);
        Assert.AreEqual(1, comments.Count);
        AssertComment(comments[0], guid, comment, [KnownData.LabelReject], mentions);
    }

    [TestMethod]
    public async Task VerifyPunchItem_AsWriter_ShouldVerifyPunchItem()
    {
        // Arrange
        var (guid, rowVersionAfterClear) = await PunchItemsControllerTestsHelper.CreateClearedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guid);
        Assert.IsNotNull(punchItem);
        Assert.IsTrue(punchItem.IsReadyToBeVerified);

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.VerifyPunchItemAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guid,
            rowVersionAfterClear);

        // Assert
        AssertRowVersionChange(rowVersionAfterClear, newRowVersion);
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guid);
        Assert.IsFalse(punchItem.IsReadyToBeVerified);
    }

    [TestMethod]
    public async Task UnverifyPunchItem_AsWriter_ShouldUnverifyPunchItem()
    {
        // Arrange
        var (guid, rowVersionAfterVerify) = await PunchItemsControllerTestsHelper.CreateVerifiedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guid);
        Assert.IsNotNull(punchItem);
        Assert.IsTrue(punchItem.IsReadyToBeUnverified);

        // Act
        var newRowVersion = await PunchItemsControllerTestsHelper.UnverifyPunchItemAsync(
            UserType.Writer, TestFactory.PlantWithAccess,
            guid,
            rowVersionAfterVerify);

        // Assert
        AssertRowVersionChange(rowVersionAfterVerify, newRowVersion);
        punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(UserType.Writer, TestFactory.PlantWithAccess, guid);
        Assert.IsFalse(punchItem.IsReadyToBeUnverified);
    }

    [TestMethod]
    public async Task GetPunchItemsByCheckListGuid_AsReader_ShouldGetPunchItems()
    {
        // Act
        var punchItems = await PunchItemsControllerTestsHelper.GetPunchItemsByCheckListGuid(
            UserType.Reader,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted
            );

        // Assert
        Assert.IsTrue(0 < punchItems.Count);
    }

    [TestMethod]
    public async Task DuplicatePunchItem_AsWriter_ShouldDuplicateWithoutAttachments()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, _)
            = await UploadNewPunchItemAttachmentOnVerifiedPunchAsync(fileName);

        // Act
        var result = await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            [TestFactory.CheckListGuidNotRestricted],
            false);

        // Assert
        Assert.AreEqual(1, result.Count);
        var newPunchItemGuid = result.ElementAt(0).Guid;
        var punchItem = await PunchItemsControllerTestsHelper.GetPunchItemAsync(
            UserType.Writer, 
            TestFactory.PlantWithAccess, 
            newPunchItemGuid);
        Assert.IsNotNull(punchItem);
        Assert.IsTrue(punchItem.IsReadyToBeCleared);

        var punchItemAttachments = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            newPunchItemGuid);
        Assert.AreEqual(0, punchItemAttachments.Count);
    }

    [TestMethod]
    public async Task DuplicatePunchItem_AsWriter_ShouldDuplicateWithAttachments()
    {
        // Arrange
        var fileName = Guid.NewGuid().ToString();
        var (punchItemGuidAndRowVersion, _)
            = await UploadNewPunchItemAttachmentOnVerifiedPunchAsync(fileName);

        // Act
        var result = await PunchItemsControllerTestsHelper.DuplicatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            [TestFactory.CheckListGuidNotRestricted],
            true);

        // Assert
        Assert.AreEqual(1, result.Count);
        var newPunchItemGuid = result.ElementAt(0).Guid;
        var punchItemAttachments = await PunchItemsControllerTestsHelper.GetPunchItemAttachmentsAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            newPunchItemGuid);
        Assert.AreEqual(1, punchItemAttachments.Count);
    }

    [TestMethod]
    public async Task PerformanceTest_For_Create_Clear()
    {
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < 100; i++)
        {
            var guidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
                UserType.Writer,
                TestFactory.PlantWithAccess,
                "PA",
                Guid.NewGuid().ToString(),
                TestFactory.CheckListGuidNotRestricted,
                TestFactory.RaisedByOrgGuid,
                TestFactory.ClearingByOrgGuid);

            await PunchItemsControllerTestsHelper.ClearPunchItemAsync(
                UserType.Writer, TestFactory.PlantWithAccess,
                guidAndRowVersion.Guid,
                guidAndRowVersion.RowVersion);
        }
        Console.WriteLine($"Ran {sw.ElapsedMilliseconds}ms");
        //  We don't Assert. Difficult to set an expected maximum limit due to running on different machines / pipeline
    }

    [TestMethod]
    public async Task PerformanceTest_For_Get()
    {
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < 100; i++)
        {
            await PunchItemsControllerTestsHelper
                .GetPunchItemAsync(UserType.Reader, 
                    TestFactory.PlantWithAccess, 
                    _punchItemGuidUnderTest);
        }
        Console.WriteLine($"Ran {sw.ElapsedMilliseconds}ms");
        //  We don't Assert. Difficult to set an expected maximum limit due to running on different machines / pipeline
    }

    private async Task<(
        GuidAndRowVersion punchItemGuidAndRowVersion, 
        GuidAndRowVersion linkGuidAndRowVersion)>CreatePunchItemLinkAsync(string title, string url)
    {
        var punchItemGuidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            Guid.NewGuid().ToString(),
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        var linkGuidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemLinkAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            title,
            url);

        return (punchItemGuidAndRowVersion, linkGuidAndRowVersion);
    }

    private async Task<(GuidAndRowVersion punchItemGuidAndRowVersion, GuidAndRowVersion commentGuidAndRowVersion)>
        CreatePunchItemCommentAsync(string text, List<string> labels, List<Guid> mentions)
    {
        var punchItemGuidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            "PA",
            Guid.NewGuid().ToString(),
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        var commentGuidAndRowVersion = await PunchItemsControllerTestsHelper.CreatePunchItemCommentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            punchItemGuidAndRowVersion.Guid,
            text,
            labels,
            mentions);

        return (punchItemGuidAndRowVersion, commentGuidAndRowVersion);
    }

    private async Task<(GuidAndRowVersion punchItemGuidAndRowVersion, GuidAndRowVersion linkGuidAndRowVersion)>
        UploadNewPunchItemAttachmentOnVerifiedPunchAsync(string fileName)
    {
        // We test on a verified punch. This to test that current user (UserType.Writer) can work with 
        // punch attachments even after punch is verified
        var (guid, rowVersionAfterVerify) = await PunchItemsControllerTestsHelper.CreateVerifiedPunchItemAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            TestFactory.CheckListGuidNotRestricted,
            TestFactory.RaisedByOrgGuid,
            TestFactory.ClearingByOrgGuid);

        var attachmentGuidAndRowVersion = await PunchItemsControllerTestsHelper.UploadNewPunchItemAttachmentAsync(
            UserType.Writer,
            TestFactory.PlantWithAccess,
            guid,
            new TestFile("blah", fileName));

        return (new GuidAndRowVersion{Guid = guid, RowVersion = rowVersionAfterVerify}, attachmentGuidAndRowVersion);
    }

    private static void AssertFirstAndOnlyLink(
        Guid punchItemGuid,
        Guid linkGuid,
        string linkRowVersion,
        string title,
        string url,
        List<LinkDto> links)
    {
        Assert.IsNotNull(links);
        Assert.AreEqual(1, links.Count);
        var link = links[0];
        Assert.AreEqual(punchItemGuid, link.ParentGuid);
        Assert.AreEqual(linkGuid, link.Guid);
        Assert.AreEqual(linkRowVersion, link.RowVersion);
        Assert.AreEqual(title, link.Title);
        Assert.AreEqual(url, link.Url);
    }

    private static void AssertFirstAndOnlyComment(
        List<CommentDto> commentDtos,
        Guid expectedParentGuid,
        Guid expectedCommentGuid,
        string expectedText,
        List<string> expectedLabels,
        List<Guid> expectedMentions)
    {
        Assert.IsNotNull(commentDtos);
        Assert.AreEqual(1, commentDtos.Count);
        var commentDto = commentDtos[0];
        Assert.AreEqual(expectedCommentGuid, commentDto.Guid);
        AssertComment(commentDto, expectedParentGuid, expectedText, expectedLabels, expectedMentions);
    }

    private static void AssertComment(
        CommentDto commentDto,
        Guid expectedParentGuid,
        string expectedText,
        List<string> expectedLabels,
        List<Guid> expectedMentions)
    {
        Assert.AreEqual(expectedParentGuid, commentDto.ParentGuid);
        Assert.AreEqual(expectedText, commentDto.Text);
        Assert.IsNotNull(commentDto.CreatedBy);
        Assert.IsNotNull(commentDto.CreatedAtUtc);
        Assert.AreEqual(expectedLabels.Count, commentDto.Labels.Count);

        Assert.IsNotNull(commentDto.Labels);
        foreach (var expectedLabel in expectedLabels)
        {
            Assert.IsTrue(commentDto.Labels.Any(l => l == expectedLabel));
        }
        
        Assert.IsNotNull(commentDto.Mentions);
        foreach (var expectedMention in expectedMentions)
        {
            Assert.IsTrue(commentDto.Mentions.Any(p => p.Guid == expectedMention));
        }
    }

    private static void AssertFirstAndOnlyAttachment(
        Guid punchItemGuid,
        Guid attachmentGuid,
        string attachmentRowVersion,
        string fileName,
        List<AttachmentDto> attachments)
    {
        Assert.IsNotNull(attachments);
        Assert.AreEqual(1, attachments.Count);
        var attachment = attachments[0];
        Assert.AreEqual(punchItemGuid, attachment.ParentGuid);
        Assert.AreEqual(attachmentGuid, attachment.Guid);
        Assert.AreEqual(attachmentRowVersion, attachment.RowVersion);
        Assert.AreEqual(fileName, attachment.FileName);
    }
}
