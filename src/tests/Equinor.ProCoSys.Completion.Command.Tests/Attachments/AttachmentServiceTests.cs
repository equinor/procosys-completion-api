using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.Attachments;

[TestClass]
public class AttachmentServiceTests : TestsBase
{
    private readonly string _blobContainer = "bc";
    private readonly string _sourceType = "Whatever";
    private readonly Guid _sourceGuid = Guid.NewGuid();
    private Mock<IAttachmentRepository> _attachmentRepositoryMock;
    private AttachmentService _dut;
    private Attachment _attachmentAddedToRepository;
    private Attachment _existingAttachment;
    private Mock<IAzureBlobService> _azureBlobServiceMock;
    private readonly string _existingFileName = "E.txt";
    private readonly string _newFileName = "N.txt";
    private readonly string _rowVersion = "AAAAAAAAABA=";

    [TestInitialize]
    public void Setup()
    {
        _attachmentRepositoryMock = new Mock<IAttachmentRepository>();
        _attachmentRepositoryMock
            .Setup(x => x.Add(It.IsAny<Attachment>()))
            .Callback<Attachment>(attachment =>
            {
                _attachmentAddedToRepository = attachment;
            });
        _existingAttachment = new Attachment(_sourceType, _sourceGuid, TestPlantA, _existingFileName);
        _attachmentRepositoryMock.Setup(a => a.TryGetAttachmentWithFilenameForSourceAsync(
                _existingAttachment.SourceGuid,
                _existingAttachment.FileName))
            .ReturnsAsync(_existingAttachment);
        _attachmentRepositoryMock.Setup(a => a.TryGetByGuidAsync(_existingAttachment.Guid))
            .ReturnsAsync(_existingAttachment);

        _azureBlobServiceMock = new Mock<IAzureBlobService>();
        var blobStorageOptionsMock = new Mock<IOptionsSnapshot<BlobStorageOptions>>();
        var blobStorageOptions = new BlobStorageOptions
        {
            BlobContainer = _blobContainer
        };
        blobStorageOptionsMock
            .Setup(x => x.Value)
            .Returns(blobStorageOptions);
        _dut = new AttachmentService(
            _attachmentRepositoryMock.Object,
            _plantProviderMock.Object,
            _unitOfWorkMock.Object,
            _azureBlobServiceMock.Object,
            blobStorageOptionsMock.Object,
            new Mock<ILogger<AttachmentService>>().Object);
    }

    #region UploadNewAsync
    [TestMethod]
    public async Task UploadNewAsync_ShouldThrowException_AndNotUploadToBlobStorage_WhenFileNameExist()
    {
        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.UploadNewAsync(_sourceType, _sourceGuid, _existingFileName, new MemoryStream(), default));

        // Assert
        _azureBlobServiceMock.Verify(a => a.UploadAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<bool>(),
            default), Times.Never);
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldAddNewAttachmentToRepository_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_sourceType, _sourceGuid, _newFileName, new MemoryStream(), default);

        // Assert
        Assert.IsNotNull(_attachmentAddedToRepository);
        Assert.AreEqual(_sourceGuid, _attachmentAddedToRepository.SourceGuid);
        Assert.AreEqual(_sourceType, _attachmentAddedToRepository.SourceType);
        Assert.AreEqual(_newFileName, _attachmentAddedToRepository.FileName);
        Assert.AreEqual(1, _attachmentAddedToRepository.RevisionNumber);
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldSaveOnce_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_sourceType, _sourceGuid, _newFileName, new MemoryStream(), default);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldAddAttachmentUploadedEvent_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_sourceType, _sourceGuid, _newFileName, new MemoryStream(), default);

        // Assert
        Assert.IsInstanceOfType(_attachmentAddedToRepository.DomainEvents.First(), typeof(NewAttachmentUploadedEvent));
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldUploadToBlobStorage_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_sourceType, _sourceGuid, _newFileName, new MemoryStream(), default);

        // Assert
        var p = _attachmentAddedToRepository.GetFullBlobPath();
        _azureBlobServiceMock.Verify(a
            => a.UploadAsync(
                _blobContainer,
                p,
                It.IsAny<Stream>(),
                false,
                default), Times.Once);
    }
    #endregion

    #region UploadOverwrite
    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldNotAddNewAttachmentToRepository_WhenFileNameExist()
    {
        // Act
        await _dut.UploadOverwriteAsync(_sourceType, _sourceGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        Assert.IsNull(_attachmentAddedToRepository);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldIncreaseRevisionNumber_WhenFileNameExist()
    {
        // Arrange
        Assert.AreEqual(1, _existingAttachment.RevisionNumber);

        // Act
        await _dut.UploadOverwriteAsync(_sourceType, _sourceGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        Assert.AreEqual(2, _existingAttachment.RevisionNumber);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldSaveOnce_WhenFileNameExist()
    {
        // Act
        await _dut.UploadOverwriteAsync(_sourceType, _sourceGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldAddExistingAttachmentUploadedAndOverwrittenEvent_WhenFileNameExist()
    {
        // Act
        await _dut.UploadOverwriteAsync(_sourceType, _sourceGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        Assert.IsInstanceOfType(_existingAttachment.DomainEvents.First(), typeof(ExistingAttachmentUploadedAndOverwrittenEvent));
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldUploadToBlobStorage_WhenFileNameExist()
    {
        // Act
        await _dut.UploadOverwriteAsync(_sourceType, _sourceGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        var p = _existingAttachment.GetFullBlobPath();
        _azureBlobServiceMock.Verify(a
            => a.UploadAsync(
                _blobContainer,
                p,
                It.IsAny<Stream>(),
                true,
                default), Times.Once);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldSetAndReturnRowVersion()
    {
        // Act
        var result = await _dut.UploadOverwriteAsync(_sourceType, _sourceGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_rowVersion, result);
        Assert.AreEqual(_rowVersion, _existingAttachment.RowVersion.ConvertToString());
    }
    #endregion

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenKnownAttachment()
    {
        // Act
        var result = await _dut.ExistsAsync(_existingAttachment.Guid);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnNull_WhenUnknownAttachment()
    {
        // Arrange
        // Act
        var result = await _dut.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region DeleteAsync
    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteAttachmentFromRepository_WhenKnownAttachment()
    {
        // Act
        await _dut.DeleteAsync(_existingAttachment.Guid, _rowVersion, default);

        // Assert
        _attachmentRepositoryMock.Verify(a => a.Remove(_existingAttachment), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteAttachmentFromBlobStorage_WhenKnownAttachment()
    {
        // Act
        await _dut.DeleteAsync(_existingAttachment.Guid, _rowVersion, default);

        // Assert
        var p = _existingAttachment.GetFullBlobPath();
        _azureBlobServiceMock.Verify(a
            => a.DeleteAsync(
                _blobContainer,
                p,
                default), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowException_WhenUnknownAttachment()
    {
        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.DeleteAsync(Guid.NewGuid(), _rowVersion, default));

        // Assert
        _attachmentRepositoryMock.Verify(
            a => a.Remove(
                It.IsAny<Attachment>()),
            Times.Never);
        _azureBlobServiceMock.Verify(
            a => a.DeleteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                default),
            Times.Never);
    }
    #endregion
}
