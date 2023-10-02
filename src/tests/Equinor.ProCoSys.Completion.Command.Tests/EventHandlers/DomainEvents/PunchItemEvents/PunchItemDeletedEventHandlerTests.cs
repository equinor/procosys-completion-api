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
public class PunchItemDeletedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemDeletedEventHandler _dut;
    private PunchItemDeletedDomainEvent _punchItemDeletedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private PunchItemDeletedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.SetModified(_person);

        _punchItemDeletedEvent = new PunchItemDeletedDomainEvent(_punchItem);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new PunchItemDeletedEventHandler(_publishEndpointMock, Substitute.For<ILogger<PunchItemDeletedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<PunchItemDeletedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<PunchItemDeletedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<PunchItemDeletedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemDeletedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemDeletedEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
         .Publish(Arg.Any<PunchItemDeletedIntegrationEvent>(),
             Arg.Any<IPipe<PublishContext<PunchItemDeletedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemDeletedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item deleted", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemDeletedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemDeletedEvent.PunchItem.ModifiedAtUtc, _publishedIntegrationEvent.DeletedAtUtc);
        Assert.AreEqual(_punchItemDeletedEvent.PunchItem.ModifiedBy!.Guid, _publishedIntegrationEvent.DeletedByOid);
    }
}
