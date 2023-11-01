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
public class LinkCreatedEventHandlerTests : EventHandlerTestBase
{
    private LinkCreatedEventHandler _dut;
    private LinkCreatedDomainEvent _linkCreatedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private LinkCreatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _link.SetCreated(_person);

        _linkCreatedEvent = new LinkCreatedDomainEvent(_link);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new LinkCreatedEventHandler(_publishEndpointMock, Substitute.For<ILogger<LinkCreatedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<LinkCreatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<LinkCreatedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<LinkCreatedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_LinkCreatedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_linkCreatedEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<LinkCreatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<LinkCreatedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_linkCreatedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual($"Link {_linkCreatedEvent.Link.Title} created", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_linkCreatedEvent.Link.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_linkCreatedEvent.Link.SourceGuid, _publishedIntegrationEvent.SourceGuid);
        Assert.AreEqual(_linkCreatedEvent.Link.SourceType, _publishedIntegrationEvent.SourceType);
        Assert.AreEqual(_linkCreatedEvent.Link.Title, _publishedIntegrationEvent.Title);
        Assert.AreEqual(_linkCreatedEvent.Link.Url, _publishedIntegrationEvent.Url);
        Assert.AreEqual(_linkCreatedEvent.Link.CreatedAtUtc, _publishedIntegrationEvent.CreatedAtUtc);
        Assert.AreEqual(_linkCreatedEvent.Link.CreatedBy.Guid, _publishedIntegrationEvent.CreatedBy.Oid);
        Assert.AreEqual(_linkCreatedEvent.Link.CreatedBy.GetFullName(), _publishedIntegrationEvent.CreatedBy.FullName);
    }
}
