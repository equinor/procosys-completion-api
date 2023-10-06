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
public class NewAttachmentUploadedEventHandlerTests : EventHandlerTestBase
{
    private NewAttachmentUploadedEventHandler _dut;
    private NewAttachmentUploadedDomainEvent _attachmentCreatedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private AttachmentCreatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _attachment.SetCreated(_person);

        _attachmentCreatedEvent = new NewAttachmentUploadedDomainEvent(_attachment);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new NewAttachmentUploadedEventHandler(_publishEndpointMock, Substitute.For<ILogger<NewAttachmentUploadedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<AttachmentCreatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<AttachmentCreatedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<AttachmentCreatedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_AttachmentCreatedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_attachmentCreatedEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<AttachmentCreatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<AttachmentCreatedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_attachmentCreatedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual($"Attachment {_attachmentCreatedEvent.Attachment.FileName} uploaded", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_attachmentCreatedEvent.Attachment.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_attachmentCreatedEvent.Attachment.SourceGuid, _publishedIntegrationEvent.SourceGuid);
        Assert.AreEqual(_attachmentCreatedEvent.Attachment.SourceType, _publishedIntegrationEvent.SourceType);
        Assert.AreEqual(_attachmentCreatedEvent.Attachment.FileName, _publishedIntegrationEvent.FileName);
        Assert.AreEqual(_attachmentCreatedEvent.Attachment.CreatedAtUtc, _publishedIntegrationEvent.CreatedAtUtc);
        Assert.AreEqual(_attachmentCreatedEvent.Attachment.CreatedBy.Guid, _publishedIntegrationEvent.CreatedByOid);
    }
}
