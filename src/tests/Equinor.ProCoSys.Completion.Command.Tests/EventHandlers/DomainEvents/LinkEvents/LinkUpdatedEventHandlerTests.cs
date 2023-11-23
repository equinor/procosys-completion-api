using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.LinkEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.LinkEvents;

[TestClass]
public class LinkUpdatedEventHandlerTests : EventHandlerTestBase
{
    private LinkUpdatedEventHandler _dut;
    private LinkUpdatedDomainEvent _linkUpdatedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private LinkUpdatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _link.SetModified(_person);

        _linkUpdatedEvent = new LinkUpdatedDomainEvent(_link, new List<IProperty>());
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new LinkUpdatedEventHandler(_publishEndpointMock, Substitute.For<ILogger<LinkUpdatedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<LinkUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<LinkUpdatedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<LinkUpdatedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_LinkUpdatedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_linkUpdatedEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<LinkUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<LinkUpdatedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_linkUpdatedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual($"Link {_linkUpdatedEvent.Link.Title} updated", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_linkUpdatedEvent.Link.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_linkUpdatedEvent.Link.ParentGuid, _publishedIntegrationEvent.ParentGuid);
        Assert.AreEqual(_linkUpdatedEvent.Link.ParentType, _publishedIntegrationEvent.ParentType);
        Assert.AreEqual(_linkUpdatedEvent.Link.Title, _publishedIntegrationEvent.Title);
        Assert.AreEqual(_linkUpdatedEvent.Link.Url, _publishedIntegrationEvent.Url);
        Assert.AreEqual(_linkUpdatedEvent.Link.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_linkUpdatedEvent.Link.ModifiedBy!.Guid, _publishedIntegrationEvent.ModifiedBy.Oid);
        Assert.AreEqual(_linkUpdatedEvent.Link.ModifiedBy!.GetFullName(), _publishedIntegrationEvent.ModifiedBy.FullName);
    }
}
