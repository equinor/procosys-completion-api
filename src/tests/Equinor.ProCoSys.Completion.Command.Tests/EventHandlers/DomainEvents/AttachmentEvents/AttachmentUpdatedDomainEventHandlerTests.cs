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
public class AttachmentUpdatedDomainEventHandlerTests : EventHandlerTestBase
{
    private AttachmentUpdatedDomainEventHandler _dut;
    private AttachmentUpdatedDomainEvent _attachmentUpdatedDomainEvent;
    private IPublishEndpoint _publishEndpointMock;
    private AttachmentUpdatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _attachment.SetModified(_person);

        _attachmentUpdatedDomainEvent = new AttachmentUpdatedDomainEvent(_attachment);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new AttachmentUpdatedDomainEventHandler(_publishEndpointMock, Substitute.For<ILogger<AttachmentUpdatedDomainEventHandler>>());
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
        await _dut.Handle(_attachmentUpdatedDomainEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<AttachmentUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<AttachmentUpdatedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_attachmentUpdatedDomainEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual($"Attachment {_attachmentUpdatedDomainEvent.Attachment.FileName} updated", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.ParentGuid, _publishedIntegrationEvent.ParentGuid);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.ParentType, _publishedIntegrationEvent.ParentType);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.FileName, _publishedIntegrationEvent.FileName);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.RevisionNumber, _publishedIntegrationEvent.RevisionNumber);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.Description, _publishedIntegrationEvent.Description);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.ModifiedBy!.Guid, _publishedIntegrationEvent.ModifiedBy.Oid);
        Assert.AreEqual(_attachmentUpdatedDomainEvent.Attachment.ModifiedBy!.GetFullName(), _publishedIntegrationEvent.ModifiedBy.FullName);
        AssertSameLabels(_attachmentUpdatedDomainEvent.Attachment.Labels, _publishedIntegrationEvent.Labels);
    }
}
