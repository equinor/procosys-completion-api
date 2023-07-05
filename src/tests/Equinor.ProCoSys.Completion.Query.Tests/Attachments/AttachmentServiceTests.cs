using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.BlobStorage;
using Moq;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Query.Tests.Attachments;

[TestClass]
public class AttachmentServiceTests : ReadOnlyTestsBase
{
    private readonly string _blobContainer = "bc";
    private Attachment _createdAttachment;
    private Guid _createdAttachmentGuid;
    private Attachment _modifiedAttachment;
    private Guid _modifiedAttachmentGuid;
    private Guid _sourceGuid;
    private Mock<IOptionsSnapshot<BlobStorageOptions>> _blobStorageOptionsMock;
    private Mock<IAzureBlobService> _azureBlobServiceMock;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _sourceGuid = Guid.NewGuid();
        _createdAttachment = new Attachment("X", _sourceGuid, TestPlantA, "t1.txt");
        _modifiedAttachment = new Attachment("X", _sourceGuid, TestPlantA, "t2.txt");

        context.Attachments.Add(_createdAttachment);
        context.Attachments.Add(_modifiedAttachment);
        context.SaveChangesAsync().Wait();
        _createdAttachmentGuid = _createdAttachment.Guid;

        _modifiedAttachment.IncreaseRevisionNumber();
        context.SaveChangesAsync().Wait();
        _modifiedAttachmentGuid = _modifiedAttachment.Guid;

        _azureBlobServiceMock = new Mock<IAzureBlobService>();
        _blobStorageOptionsMock = new Mock<IOptionsSnapshot<BlobStorageOptions>>();
        var blobStorageOptions = new BlobStorageOptions
        {
            BlobContainer = _blobContainer
        };
        _blobStorageOptionsMock
            .Setup(x => x.Value)
            .Returns(blobStorageOptions);
    }

    [TestMethod]
    public async Task GetAllForSourceAsync_ShouldReturnCorrect_CreatedDtos()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new AttachmentService(context, _azureBlobServiceMock.Object, _blobStorageOptionsMock.Object);

        // Act
        var result = await dut.GetAllForSourceAsync(_sourceGuid, default);

        // Assert
        var attachmentDtos = result.ToList();
        Assert.AreEqual(2, attachmentDtos.Count);
        var createdAttachmentDto = attachmentDtos.SingleOrDefault(a => a.Guid == _createdAttachmentGuid);
        Assert.IsNotNull(createdAttachmentDto);
        AssertAttachmentDto(_createdAttachment, createdAttachmentDto);
        Assert.IsNull(createdAttachmentDto.ModifiedBy);
        Assert.IsNull(createdAttachmentDto.ModifiedAtUtc);

        var modifiedAttachmentDto = attachmentDtos.SingleOrDefault(a => a.Guid == _modifiedAttachmentGuid);
        Assert.IsNotNull(modifiedAttachmentDto);
        AssertAttachmentDto(_modifiedAttachment, modifiedAttachmentDto);

        var modifiedBy = modifiedAttachmentDto.ModifiedBy;
        Assert.IsNotNull(modifiedBy);
        Assert.AreEqual(CurrentUserOid, modifiedBy.Guid);
        Assert.IsTrue(modifiedAttachmentDto.ModifiedAtUtc.HasValue);
        Assert.AreEqual(_modifiedAttachment.ModifiedAtUtc, modifiedAttachmentDto.ModifiedAtUtc);
    }

    [TestMethod]
    public async Task GetDownloadUriAsync_ShouldReturnUri_WhenKnownAttachment()
    {
        // Arrange
        var uri = new Uri("http://blah.blah.com");
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new AttachmentService(context, _azureBlobServiceMock.Object, _blobStorageOptionsMock.Object);
        var p = _createdAttachment.GetFullBlobPath();
        _azureBlobServiceMock.Setup(a => a.GetDownloadSasUri(_blobContainer, p, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Returns(uri);

        // Act
        var result = await dut.GetDownloadUriAsync(_createdAttachment.Guid, default);

        Assert.IsNotNull(result);
        Assert.AreEqual(uri, result);
    }


    [TestMethod]
    public async Task GetDownloadUriAsync_ShouldReturnNull_WhenUnknownAttachment()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new AttachmentService(context, _azureBlobServiceMock.Object, _blobStorageOptionsMock.Object);

        // Act
        var result = await dut.GetDownloadUriAsync(Guid.NewGuid(), default);

        Assert.IsNull(result);
        _azureBlobServiceMock.Verify(
            a => a.GetDownloadSasUri(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()), Times.Never);
    }

    private void AssertAttachmentDto(Attachment attachment, AttachmentDto attachmentDto)
    {
        Assert.AreEqual(attachment.SourceGuid, attachmentDto.SourceGuid);
        Assert.AreEqual(attachment.Guid, attachmentDto.Guid);
        Assert.AreEqual(attachment.GetFullBlobPath(), attachmentDto.FullBlobPath);
        Assert.AreEqual(attachment.FileName, attachmentDto.FileName);
        var createdBy = attachmentDto.CreatedBy;
        Assert.IsNotNull(createdBy);
        Assert.AreEqual(CurrentUserOid, createdBy.Guid);
        Assert.AreEqual(attachment.CreatedAtUtc, attachmentDto.CreatedAtUtc);
    }
}
