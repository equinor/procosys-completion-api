﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
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
            });

        _existingAttachment = new Attachment(_parentType, _parentGuid, TestPlantA, _existingFileName);

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
            Substitute.For<ILogger<AttachmentService>>());
    }

    #region UploadNewAsync
    [TestMethod]
    public async Task UploadNewAsync_ShouldThrowException_AndNotUploadToBlobStorage_WhenFileNameExist()
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
        await  _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldAddAttachmentUploadedEvent_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, new MemoryStream(), default);

        // Assert
        Assert.IsInstanceOfType(_attachmentAddedToRepository.DomainEvents.First(), typeof(NewAttachmentUploadedDomainEvent));
    }

    [TestMethod]
    public async Task UploadNewAsync_ShouldUploadToBlobStorage_WhenFileNameNotExist()
    {
        // Act
        await _dut.UploadNewAsync(_parentType, _parentGuid, _newFileName, new MemoryStream(), default);

        // Assert
        var p = _attachmentAddedToRepository.GetFullBlobPath();
       await _azureBlobServiceMock.Received(1).UploadAsync(
            _blobContainer,
            p,
            Arg.Any<Stream>());

    }
    #endregion

    #region UploadOverwrite
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
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldAddExistingAttachmentUploadedAndOverwrittenEvent_WhenFileNameExist()
    {
        // Act
        await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        Assert.IsInstanceOfType(_existingAttachment.DomainEvents.First(), typeof(ExistingAttachmentUploadedAndOverwrittenDomainEvent));
    }

    [TestMethod]
    public async Task UploadOverwriteAsync_ShouldUploadToBlobStorage_WhenFileNameExist()
    {
        // Act
        await _dut.UploadOverwriteAsync(_parentType, _parentGuid, _existingFileName, new MemoryStream(), _rowVersion, default);

        // Assert
        var p = _existingAttachment.GetFullBlobPath();
        await _azureBlobServiceMock.Received(1)
           .UploadAsync(
                _blobContainer,
                p,
                Arg.Any<Stream>(),
                true);
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
    #endregion

    #region Update
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
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldAddAttachmentUpdatedDomainEvent()
    {
        // Act
        await _dut.UpdateAsync(_existingAttachment.Guid, "abc", new List<Label>(), _rowVersion, default);

        // Assert
        Assert.IsInstanceOfType(_existingAttachment.DomainEvents.First(), typeof(AttachmentUpdatedDomainEvent));
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldNotAddAttachmentUpdatedDomainEvent_WhenNoChanges()
    {
        // Act
        await _dut.UpdateAsync(
            _existingAttachment.Guid,
            _existingAttachment.Description,
            new List<Label>(),
            _rowVersion,
            default);

        // Assert
        var attachmentUpdatedDomainEventAdded =
            _existingAttachment.DomainEvents.Any(e => e.GetType() == typeof(AttachmentUpdatedDomainEvent));
        Assert.IsFalse(attachmentUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldAddCorrectAttachmentUpdatedDomainEvent_WhenChanges()
    {
        // Arrange
        var oldTitle = _existingAttachment.Description;
        var newTitle = Guid.NewGuid().ToString();

        // Act
        await _dut.UpdateAsync(
            _existingAttachment.Guid,
            newTitle,
            new List<Label>(),
            _rowVersion,
            default);

        // Assert
        var attachmentUpdatedDomainEventAdded = _existingAttachment.DomainEvents.Last() as AttachmentUpdatedDomainEvent;
        Assert.IsNotNull(attachmentUpdatedDomainEventAdded);
        Assert.IsNotNull(attachmentUpdatedDomainEventAdded.Changes);
        var change = attachmentUpdatedDomainEventAdded
            .Changes
            .SingleOrDefault(c => c.Name == nameof(Attachment.Description));
        Assert.IsNotNull(change);
        Assert.AreEqual(oldTitle, change.OldValue);
        Assert.AreEqual(newTitle, change.NewValue);
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
