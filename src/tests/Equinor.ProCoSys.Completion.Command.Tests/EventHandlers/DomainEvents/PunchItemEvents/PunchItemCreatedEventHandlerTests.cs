using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.PunchItemEvents;

[TestClass]
public class PunchItemCreatedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemCreatedEventHandler _dut;
    private PunchItemCreatedDomainEvent _punchItemCreatedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private PunchItemCreatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.SetCreated(_person);

        _punchItemCreatedEvent = new PunchItemCreatedDomainEvent(_punchItem);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new PunchItemCreatedEventHandler(_publishEndpointMock, Substitute.For<ILogger<PunchItemCreatedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<PunchItemCreatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemCreatedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<PunchItemCreatedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemCreatedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemCreatedEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<PunchItemCreatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemCreatedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemCreatedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item created", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.Project.Guid, _publishedIntegrationEvent.ProjectGuid);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.CreatedAtUtc, _publishedIntegrationEvent.CreatedAtUtc);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.CreatedBy.Guid, _publishedIntegrationEvent.CreatedByOid);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.ItemNo, _publishedIntegrationEvent.ItemNo);
    }
}
