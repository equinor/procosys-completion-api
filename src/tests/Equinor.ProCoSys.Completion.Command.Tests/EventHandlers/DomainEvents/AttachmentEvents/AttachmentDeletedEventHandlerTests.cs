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
public class AttachmentDeletedEventHandlerTests : EventHandlerTestBase
{
    private AttachmentDeletedEventHandler _dut;
    private AttachmentDeletedDomainEvent _attachmentDeletedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private AttachmentDeletedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        // Need to simulate what CompletionContext.SaveChangesAsync do, since it set ...
        // ... both ModifiedBy and ModifiedAtUtc when entity is deleted
        _attachment.SetModified(_person);

        _attachmentDeletedEvent = new AttachmentDeletedDomainEvent(_attachment);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new AttachmentDeletedEventHandler(_publishEndpointMock, Substitute.For<ILogger<AttachmentDeletedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<AttachmentDeletedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<AttachmentDeletedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<AttachmentDeletedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_AttachmentDeletedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_attachmentDeletedEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<AttachmentDeletedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<AttachmentDeletedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_attachmentDeletedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual($"Attachment {_attachmentDeletedEvent.Attachment.FileName} deleted", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_attachmentDeletedEvent.Attachment.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_attachmentDeletedEvent.Attachment.SourceGuid, _publishedIntegrationEvent.ParentGuid);

        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... use ModifiedBy/ModifiedAtUtc which is set when saving a delete
        Assert.AreEqual(_attachmentDeletedEvent.Attachment.ModifiedAtUtc, _publishedIntegrationEvent.DeletedAtUtc);
        Assert.AreEqual(_attachmentDeletedEvent.Attachment.ModifiedBy!.Guid, _publishedIntegrationEvent.DeletedBy.Oid);
        Assert.AreEqual(_attachmentDeletedEvent.Attachment.ModifiedBy!.GetFullName(), _publishedIntegrationEvent.DeletedBy.FullName);
    }
}
