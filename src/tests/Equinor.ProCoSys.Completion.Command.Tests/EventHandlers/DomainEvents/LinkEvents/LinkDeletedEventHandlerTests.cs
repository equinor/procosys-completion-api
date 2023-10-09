using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.LinkEvents;

[TestClass]
public class LinkDeletedEventHandlerTests : EventHandlerTestBase
{
    private LinkDeletedEventHandler _dut;
    private LinkDeletedDomainEvent _linkDeletedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private LinkDeletedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        // Need to simulate what CompletionContext.SaveChangesAsync do, since it set ...
        // ... both ModifiedBy and ModifiedAtUtc when entity is deleted
        _link.SetModified(_person);

        _linkDeletedEvent = new LinkDeletedDomainEvent(_link);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new LinkDeletedEventHandler(_publishEndpointMock, Substitute.For<ILogger<LinkDeletedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<LinkDeletedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<LinkDeletedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<LinkDeletedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_LinkDeletedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_linkDeletedEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<LinkDeletedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<LinkDeletedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_linkDeletedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual($"Link {_linkDeletedEvent.Link.Title} deleted", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_linkDeletedEvent.Link.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_linkDeletedEvent.Link.SourceGuid, _publishedIntegrationEvent.SourceGuid);

        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... use ModifiedBy/ModifiedAtUtc which is set when saving a delete
        Assert.AreEqual(_linkDeletedEvent.Link.ModifiedAtUtc, _publishedIntegrationEvent.DeletedAtUtc);
        Assert.AreEqual(_linkDeletedEvent.Link.ModifiedBy!.Guid, _publishedIntegrationEvent.DeletedByOid);
    }
}
