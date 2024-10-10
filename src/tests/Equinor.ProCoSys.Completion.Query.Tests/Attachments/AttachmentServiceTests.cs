using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using NSubstitute;
using Microsoft.Extensions.Options;
using Equinor.ProCoSys.Completion.Domain;
using Azure.Storage.Blobs.Models;
using Equinor.ProCoSys.Completion.Query.UserDelegationProvider;

namespace Equinor.ProCoSys.Completion.Query.Tests.Attachments;

[TestClass]
public class AttachmentServiceTests : ReadOnlyTestsBase
{
    private readonly string _blobContainer = "bc";
    private Attachment _createdAttachment;
    private Guid _createdAttachmentGuid;
    private Attachment _modifiedAttachment;
    private Guid _modifiedAttachmentGuid;
    private Guid _parentGuid;
    private IOptionsSnapshot<BlobStorageOptions> _blobStorageOptionsMock;
    private IAzureBlobService _azureBlobServiceMock;
    private IUserDelegationProvider _userDelegationProviderMock;
    private IOptionsSnapshot<ApplicationOptions> _applicationOptionsMock;

    protected override async void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        await using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        await Add4UnorderedLabelsInclusiveAVoidedLabelAsync(context);

        var labelA = context.Labels.Single(l => l.Text == LabelTextA);
        var labelB = context.Labels.Single(l => l.Text == LabelTextB);
        var labelC = context.Labels.Single(l => l.Text == LabelTextC);
        var voidedLabel = context.Labels.Single(l => l.Text == LabelTextVoided);

        _parentGuid = Guid.NewGuid();
        _createdAttachment = new Attachment("Proj", "X", _parentGuid, "t1.txt");
        // insert labels non-ordered to test ordering
        _createdAttachment.UpdateLabels(new List<Label> { labelB, voidedLabel, labelC, labelA });
        _modifiedAttachment = new Attachment("Proj", "X", _parentGuid, "t2.txt");

        context.Attachments.Add(_createdAttachment);
        context.Attachments.Add(_modifiedAttachment);
        context.SaveChangesAsync().Wait();
        _createdAttachmentGuid = _createdAttachment.Guid;

        _modifiedAttachment.IncreaseRevisionNumber();
        context.SaveChangesAsync().Wait();
        _modifiedAttachmentGuid = _modifiedAttachment.Guid;

        _azureBlobServiceMock = Substitute.For<IAzureBlobService>();

        var _userDelegationKeyMock = Substitute.For<UserDelegationKey>();
        _userDelegationProviderMock = Substitute.For<IUserDelegationProvider>();
        _userDelegationProviderMock.GetUserDelegationKey().Returns(_userDelegationKeyMock);

        _blobStorageOptionsMock = Substitute.For<IOptionsSnapshot<BlobStorageOptions>>();
        var blobStorageOptions = new BlobStorageOptions
        {
            BlobContainer = _blobContainer
        };
        _blobStorageOptionsMock
            .Value
            .Returns(blobStorageOptions);

        _applicationOptionsMock = Substitute.For<IOptionsSnapshot<ApplicationOptions>>();
        var appOptions = new ApplicationOptions
        {
            DevOnLocalhost = false,
        };
        _applicationOptionsMock.Value.Returns(appOptions);
    }

    [TestMethod]
    public async Task GetAllForParentAsync_ShouldReturnCorrect_CreatedDtos()
    {
        // Arrange
        var fromIpAddress = "0.0.0.0";
        var toIpAddress = "0.0.0.1";
        var uri = new Uri("http://blah.blah.com");
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        _azureBlobServiceMock.GetDownloadSasUri(_blobContainer, Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<UserDelegationKey>(), fromIpAddress, toIpAddress).Returns(uri);

        var dut = new AttachmentService(context, _azureBlobServiceMock, _userDelegationProviderMock, _blobStorageOptionsMock, _applicationOptionsMock);

        // Act
        var result = await dut.GetAllForParentAsync(_parentGuid, default, fromIpAddress, toIpAddress);

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
    public async Task ExistsAsync_ShouldReturnTrue_WhenKnownAttachment()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new AttachmentService(context, _azureBlobServiceMock, _userDelegationProviderMock, _blobStorageOptionsMock, _applicationOptionsMock);

        // Act
        var result = await dut.ExistsAsync(_createdAttachment.Guid, default);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnNull_WhenUnknownAttachment()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new AttachmentService(context, _azureBlobServiceMock, _userDelegationProviderMock, _blobStorageOptionsMock, _applicationOptionsMock);

        // Act
        var result = await dut.ExistsAsync(Guid.NewGuid(), default);

        Assert.IsFalse(result);
    }

    private void AssertAttachmentDto(Attachment attachment, AttachmentDto attachmentDto)
    {
        Assert.AreEqual(attachment.ParentGuid, attachmentDto.ParentGuid);
        Assert.AreEqual(attachment.Guid, attachmentDto.Guid);
        Assert.AreEqual(attachment.GetFullBlobPath(), attachmentDto.FullBlobPath);
        Assert.AreEqual(attachment.FileName, attachmentDto.FileName);
        Assert.AreEqual(attachment.Description, attachmentDto.Description);
        var createdBy = attachmentDto.CreatedBy;
        Assert.IsNotNull(createdBy);
        Assert.AreEqual(CurrentUserOid, createdBy.Guid);
        Assert.AreEqual(attachment.CreatedAtUtc, attachmentDto.CreatedAtUtc);

        AssertOrderedNonVoidedLabels(attachment, attachmentDto);
    }

    private static void AssertOrderedNonVoidedLabels(Attachment attachment, AttachmentDto attachmentDto)
    {
        Assert.IsNotNull(attachmentDto.Labels);
        var expectedLabels = attachment.Labels.Where(l => !l.IsVoided).ToList();
        Assert.AreEqual(expectedLabels.Count, attachmentDto.Labels.Count);
        foreach (var label in expectedLabels)
        {
            Assert.IsTrue(attachmentDto.Labels.Any(l => l == label.Text));
        }
    }
}
