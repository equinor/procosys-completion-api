using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Attachments;

[TestClass]
public class AttachmentServiceTests : TestsBase
{
    private readonly string _blobContainer = "bc";
    private readonly string _parentType = "Whatever";
    private readonly Guid _parentGuid = Guid.NewGuid();
    private IAttachmentRepository _attachmentRepositoryMock;
    private AttachmentService _dut;
    private Attachment _attachmentAddedToRepository;
    private Attachment _existingAttachment;
    private IAzureBlobService _azureBlobServiceMock;
    private IIntegrationEventPublisher _integrationEventPublisherMock;
    private readonly string _existingFileName = "E.txt";
    private readonly string _newFileName = "N.txt";
    private readonly string _rowVersion = "AAAAAAAAABA=";

    [TestInitialize]
    public void Setup()
    {
        _attachmentRepositoryMock = Substitute.For<IAttachmentRepository>();
        _attachmentRepositoryMock
            .When(x => x.Add(Arg.Any<Attachment>()))
            .Do(callInfo =>
            {
                _attachmentAddedToRepository = callInfo.Arg<Attachment>();
                _attachmentAddedToRepository.SetCreated(_person);
            });

        _existingAttachment = new Attachment(_parentType, _parentGuid, TestPlantA, _existingFileName);
        _existingAttachment.SetCreated(_person);
        _existingAttachment.SetModified(_person);

        _attachmentRepositoryMock
            .GetAttachmentWithFileNameForParentAsync(_existingAttachment.ParentGuid, _existingAttachment.FileName, default)
            .Returns(_existingAttachment);

        _attachmentRepositoryMock
            .GetAsync(_existingAttachment.Guid, default)
            .Returns(_existingAttachment);

        _attachmentRepositoryMock.GetAttachmentWithFileNameForParentAsync(
                _existingAttachment.ParentGuid,
                _existingAttachment.FileName,
                default)
            .Returns(_existingAttachment);

        _attachmentRepositoryMock.GetAsync(_existingAttachment.Guid, default)
            .Returns(_existingAttachment);

        _azureBlobServiceMock = Substitute.For<IAzureBlobService>();
        var blobStorageOptionsMock = Substitute.For<IOptionsSnapshot<BlobStorageOptions>>();

        _integrationEventPublisherMock = Substitute.For<IIntegrationEventPublisher>();

        var blobStorageOptions = new BlobStorageOptions
        {
            BlobContainer = _blobContainer
        };
            
        blobStorageOptionsMock.Value.Returns(blobStorageOptions);

        _dut = new AttachmentService(
            _attachmentRepositoryMock,
            _plantProviderMock,
            _unitOfWorkMock,
            _azureBlobServiceMock,
            blobStorageOptionsMock,
            _integrationEventPublisherMock,
            Substitute.For<ILogger<AttachmentService>>());
    }

    #region UploadNewAsync
    [TestMethod]
    public async Task UploadNewAsync_ShouldThrowException_AndNotPerformAnything_WhenFileNameExist()
    {
        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.UploadNewAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), default));

        // Assert
       await _azureBlobServiceMock.Received(0).UploadAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Stream>(),
            Arg.Any<bool>());
       await _integrationEventPublisherMock.Received(0)
           .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
       await _unitOfWorkMock.Received(0).SetAuditDataAsync();
       await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldAddNewAttachmentToRepository_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, new MemoryStream(), default);

        // Assert
        Assert.IsNotNull(_attachmentAddedToRepository);
        Assert.AreEqual(_parentGuid, _attachmentAddedToRepository.ParentGuid);
        Assert.AreEqual(_parentType, _attachmentAddedToRepository.ParentType);
        Assert.AreEqual(_newFileName, _attachmentAddedToRepository.FileName);
        Assert.AreEqual(1, _attachmentAddedToRepository.RevisionNumber);
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldAddNewAttachmentToRepository_WithoutLabels()
    {
        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, new MemoryStream(), default);

        // Assert
        Assert.IsNotNull(_attachmentAddedToRepository);
        Assert.AreEqual(0, _attachmentAddedToRepository.Labels.Count);
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldSaveOnce_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, new MemoryStream(), default);

        // Assert
        await  _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldSetAuditDataAsyncOnce_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, new MemoryStream(), default);

        // Assert
        await _unitOfWorkMock.Received(1).SetAuditDataAsync();
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldPublishAttachmentCreatedIntegrationEvent_WhenFileNameNotExist()
    {
        // Arrange
        AttachmentCreatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<AttachmentCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<AttachmentCreatedIntegrationEvent>();
            }));

        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, new MemoryStream(), default);

        // Assert
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(TestPlantA, integrationEvent.Plant);
        Assert.AreEqual(_attachmentAddedToRepository.Guid, integrationEvent.Guid);
        Assert.AreEqual(_attachmentAddedToRepository.ParentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_attachmentAddedToRepository.ParentType, integrationEvent.ParentType);
        Assert.AreEqual(_attachmentAddedToRepository.BlobPath, integrationEvent.BlobPath);
        Assert.AreEqual(_attachmentAddedToRepository.FileName, integrationEvent.FileName);
        Assert.AreEqual(_attachmentAddedToRepository.CreatedAtUtc, integrationEvent.CreatedAtUtc);
        Assert.AreEqual(_attachmentAddedToRepository.CreatedBy.Guid, integrationEvent.CreatedBy.Oid);
        Assert.AreEqual(_attachmentAddedToRepository.CreatedBy.GetFullName(), integrationEvent.CreatedBy.FullName);
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldPublishHistoryCreatedIntegrationEvent_WhenFileNameNotExist()
    {
        // Arrange
        HistoryCreatedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryCreatedIntegrationEvent>();
            }));

        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, new MemoryStream(), default);

        // Assert
        AssertHistoryCreatedIntegrationEvent(
            historyEvent,
            $"Attachment {_attachmentAddedToRepository.FileName} uploaded",
            _attachmentAddedToRepository.ParentGuid,
            _attachmentAddedToRepository,
            _attachmentAddedToRepository);

        Assert.AreEqual(1, historyEvent.Properties.Count);
        AssertProperty(
            historyEvent.Properties
                .SingleOrDefault(c => c.Name == nameof(Attachment.FileName)),
            _attachmentAddedToRepository.FileName);
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldUploadToBlobStorage_WhenFileNameNotExist()
    {
        // Arrange
        var stream = new MemoryStream();
        
        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, stream, default);

        // Assert
        var p = _attachmentAddedToRepository.GetFullBlobPath();
       await _azureBlobServiceMock.Received(1).UploadAsync(_blobContainer, p, stream);

    }
    #endregion

    #region UploadOverwriteAsync
    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldThrowException_AndNotPerformAnything_WhenAttachmentNotExist()
    {
        // Arrange
        _attachmentRepositoryMock
            .GetAttachmentWithFileNameForParentAsync(_parentGuid, _existingFileName, default)
            .Returns((Attachment)null);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default));

        // Assert
        await _azureBlobServiceMock.Received(0).UploadAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Stream>(),
            Arg.Any<bool>());
        await _integrationEventPublisherMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(0).SetAuditDataAsync();
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldNotAddNewAttachmentToRepository_WhenFileNameExist()
    {
        // Act
        await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        Assert.IsNull(_attachmentAddedToRepository);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldIncreaseRevisionNumber_WhenFileNameExist()
    {
        // Arrange
        Assert.AreEqual(1, _existingAttachment.RevisionNumber);

        // Act
        await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        Assert.AreEqual(2, _existingAttachment.RevisionNumber);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldSaveOnce_WhenFileNameExist()
    {
        // Act
        await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldPublishAttachmentUpdatedIntegrationEvent_WhenFileNameExist()
    {
        // Arrange
        AttachmentUpdatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<AttachmentUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<AttachmentUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(TestPlantA, integrationEvent.Plant);
        Assert.AreEqual(_existingAttachment.Guid, integrationEvent.Guid);
        Assert.AreEqual(_existingAttachment.ParentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_existingAttachment.ParentType, integrationEvent.ParentType);
        Assert.AreEqual(_existingAttachment.BlobPath, integrationEvent.BlobPath);
        Assert.AreEqual(_existingAttachment.FileName, integrationEvent.FileName);
        Assert.AreEqual(_existingAttachment.RevisionNumber, integrationEvent.RevisionNumber);
        Assert.AreEqual(_existingAttachment.ModifiedAtUtc, integrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_existingAttachment.ModifiedBy!.Guid, integrationEvent.ModifiedBy.Oid);
        Assert.AreEqual(_existingAttachment.ModifiedBy!.GetFullName(), integrationEvent.ModifiedBy.FullName);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldPublishHistoryUpdatedIntegrationEvent_WhenFileNameExist()
    {
        // Arrange
        var oldRevisionNumber = _existingAttachment.RevisionNumber;
        HistoryUpdatedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            _plantProviderMock.Plant,
            $"Attachment {_existingAttachment.FileName} uploaded again",
            _existingAttachment,
            _existingAttachment,
            _existingAttachment.ParentGuid);
        Assert.AreEqual(1, historyEvent.ChangedProperties.Count);
        AssertChange(
            historyEvent.ChangedProperties
                .SingleOrDefault(c => c.Name == nameof(Attachment.RevisionNumber)),
            oldRevisionNumber,
            _existingAttachment.RevisionNumber);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldUploadToBlobStorage_WhenFileNameExist()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, stream, _rowVersion, default);

        // Assert
        var p = _existingAttachment.GetFullBlobPath();
        await _azureBlobServiceMock.Received(1).UploadAsync(_blobContainer, p, stream, true);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldSetAndReturnRowVersion()
    {
        // Act
        var result = await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

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
        // Arrange
        _attachmentRepositoryMock.ExistsAsync(_existingAttachment.Guid, default)
            .Returns(true);

        // Act
        var result = await _dut.ExistsAsync(_existingAttachment.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnNull_WhenUnknownAttachment()
    {
        // Arrange
        // Act
        var result = await _dut.ExistsAsync(Guid.NewGuid(), default);

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
        _attachmentRepositoryMock.Received(1).Remove(_existingAttachment);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteAttachmentFromBlobStorage_WhenKnownAttachment()
    {
        // Act
        await _dut.DeleteAsync(_existingAttachment.Guid, _rowVersion, default);

        // Assert
        var p = _existingAttachment.GetFullBlobPath();
        await _azureBlobServiceMock.Received(1).DeleteAsync(
                _blobContainer,
                p);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.DeleteAsync(_existingAttachment.Guid, _rowVersion, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldPublishAttachmentDeletedIntegrationEvent()
    {
        // Arrange
        AttachmentDeletedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<AttachmentDeletedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<AttachmentDeletedIntegrationEvent>();
            }));

        // Act
        await _dut.DeleteAsync(_existingAttachment.Guid, _rowVersion, default);

        // Assert
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(TestPlantA, integrationEvent.Plant);
        Assert.AreEqual(_existingAttachment.Guid, integrationEvent.Guid);
        Assert.AreEqual(_existingAttachment.Guid, integrationEvent.Guid);
        Assert.AreEqual(_existingAttachment.ParentGuid, integrationEvent.ParentGuid);

        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... use ModifiedBy/ModifiedAtUtc which is set when saving a deletion
        Assert.AreEqual(_existingAttachment.ModifiedAtUtc, integrationEvent.DeletedAtUtc);
        Assert.AreEqual(_existingAttachment.ModifiedBy!.Guid, integrationEvent.DeletedBy.Oid);
        Assert.AreEqual(_existingAttachment.ModifiedBy!.GetFullName(), integrationEvent.DeletedBy.FullName);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldPublishHistoryDeletedIntegrationEvent()
    {
        // Arrange
        HistoryDeletedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryDeletedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryDeletedIntegrationEvent>();
            }));

        // Act
        await _dut.DeleteAsync(_existingAttachment.Guid, _rowVersion, default);

        // Assert
        AssertHistoryDeletedIntegrationEvent(
            historyEvent,
            _plantProviderMock.Plant,
            $"Attachment {_existingAttachment.FileName} deleted",
            _existingAttachment.ParentGuid,
            _existingAttachment,
            _existingAttachment);
    }
    #endregion

    #region UpdateAsync
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateDescription()
    {
        // Arrange 
        var description = "abc";

        // Act
        await _dut.UpdateAsync(_existingAttachment.Guid, description, new List<Label>(), _rowVersion, default);

        // Assert
        Assert.AreEqual(description, _existingAttachment.Description);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateLabels()
    {
        // Arrange 
        var labelA = new Label("a");
        var labelB = new Label("b");

        // Act
        await _dut.UpdateAsync(_existingAttachment.Guid, "", new List<Label>{labelA, labelB}, _rowVersion, default);

        // Assert
        Assert.AreEqual(2, _existingAttachment.Labels.Count);
        Assert.AreEqual(2, _existingAttachment.GetOrderedNonVoidedLabels().Count());
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.UpdateAsync(_existingAttachment.Guid, "abc", new List<Label>(), _rowVersion, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldNotPublishAnyIntegrationEvents_WhenNoChanges()
    {
        // Act
        await _dut.UpdateAsync(
            _existingAttachment.Guid,
            _existingAttachment.Description,
            new List<Label>(),
            _rowVersion,
            default);

        // Assert
        await _integrationEventPublisherMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldPublishAttachmentUpdatedIntegrationEvent_WhenChanges()
    {
        // Arrange
        var newTitle = Guid.NewGuid().ToString();
        AttachmentUpdatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<AttachmentUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<AttachmentUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.UpdateAsync(
            _existingAttachment.Guid,
            newTitle,
            new List<Label>(),
            _rowVersion,
            default);

        // Assert
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(TestPlantA, integrationEvent.Plant);
        Assert.AreEqual(_existingAttachment.Guid, integrationEvent.Guid);
        Assert.AreEqual(_existingAttachment.ParentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_existingAttachment.ParentType, integrationEvent.ParentType);
        Assert.AreEqual(_existingAttachment.BlobPath, integrationEvent.BlobPath);
        Assert.AreEqual(_existingAttachment.FileName, integrationEvent.FileName);
        Assert.AreEqual(_existingAttachment.RevisionNumber, integrationEvent.RevisionNumber);
        Assert.AreEqual(_existingAttachment.ModifiedAtUtc, integrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_existingAttachment.ModifiedBy!.Guid, integrationEvent.ModifiedBy.Oid);
        Assert.AreEqual(_existingAttachment.ModifiedBy!.GetFullName(), integrationEvent.ModifiedBy.FullName);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldPublishHistoryUpdatedIntegrationEvent_WhenChanges()
    {
        // Arrange
        var oldDescription = _existingAttachment.Description;
        HistoryUpdatedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.UpdateAsync(
            _existingAttachment.Guid,
            Guid.NewGuid().ToString(),
            new List<Label>(),
            _rowVersion,
            default);

        // Assert
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            _plantProviderMock.Plant,
            $"Attachment {_existingAttachment.FileName} updated",
            _existingAttachment,
            _existingAttachment,
            _existingAttachment.ParentGuid);

        Assert.AreEqual(1, historyEvent.ChangedProperties.Count);
        AssertChange(
            historyEvent.ChangedProperties
                .SingleOrDefault(c => c.Name == nameof(Attachment.Description)),
            oldDescription,
            _existingAttachment.Description);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSetAndReturnRowVersion()
    {
        // Act
        var result = await _dut.UpdateAsync(_existingAttachment.Guid, "abc", new List<Label>(), _rowVersion, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_rowVersion, result);
        Assert.AreEqual(_rowVersion, _existingAttachment.RowVersion.ConvertToString());
    }
    #endregion
}
