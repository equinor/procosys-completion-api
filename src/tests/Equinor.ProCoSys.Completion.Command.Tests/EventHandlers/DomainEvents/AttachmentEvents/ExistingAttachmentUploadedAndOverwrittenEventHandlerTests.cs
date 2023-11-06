using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.AttachmentEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.AttachmentEvents;

[TestClass]
public class ExistingAttachmentUploadedAndOverwrittenEventHandlerTests : EventHandlerTestBase
{
    private ExistingAttachmentUploadedAndOverwrittenEventHandler _dut;
    private ExistingAttachmentUploadedAndOverwrittenDomainEvent _existingAttachmentUploadedAndOverwrittenDomainEvent;
    private IPublishEndpoint _publishEndpointMock;
    private AttachmentUpdatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _attachment.SetModified(_person);

        _existingAttachmentUploadedAndOverwrittenDomainEvent = new ExistingAttachmentUploadedAndOverwrittenDomainEvent(_attachment);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new ExistingAttachmentUploadedAndOverwrittenEventHandler(_publishEndpointMock, Substitute.For<ILogger<ExistingAttachmentUploadedAndOverwrittenEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<AttachmentUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<AttachmentUpdatedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<AttachmentUpdatedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_AttachmentUpdatedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_existingAttachmentUploadedAndOverwrittenDomainEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<AttachmentUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<AttachmentUpdatedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_existingAttachmentUploadedAndOverwrittenDomainEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual($"Attachment {_existingAttachmentUploadedAndOverwrittenDomainEvent.Attachment.FileName} uploaded again", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_existingAttachmentUploadedAndOverwrittenDomainEvent.Attachment.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_existingAttachmentUploadedAndOverwrittenDomainEvent.Attachment.SourceGuid, _publishedIntegrationEvent.ParentGuid);
        Assert.AreEqual(_existingAttachmentUploadedAndOverwrittenDomainEvent.Attachment.SourceType, _publishedIntegrationEvent.ParentType);
        Assert.AreEqual(_existingAttachmentUploadedAndOverwrittenDomainEvent.Attachment.FileName, _publishedIntegrationEvent.FileName);
        Assert.AreEqual(_existingAttachmentUploadedAndOverwrittenDomainEvent.Attachment.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_existingAttachmentUploadedAndOverwrittenDomainEvent.Attachment.ModifiedBy!.Guid, _publishedIntegrationEvent.ModifiedBy.Oid);
        Assert.AreEqual(_existingAttachmentUploadedAndOverwrittenDomainEvent.Attachment.ModifiedBy!.GetFullName(), _publishedIntegrationEvent.ModifiedBy.FullName);
    }
}
