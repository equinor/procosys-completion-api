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
public class PunchItemRejectedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemRejectedEventHandler _dut;
    private PunchItemRejectedDomainEvent _punchItemRejectedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private PunchItemRejectedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.Clear(_person);
        _punchItem.Reject(_person);
        _punchItem.SetModified(_person);

        _punchItemRejectedEvent = new PunchItemRejectedDomainEvent(_punchItem);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new PunchItemRejectedEventHandler(_publishEndpointMock,  Substitute.For<ILogger<PunchItemRejectedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<PunchItemRejectedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemRejectedIntegrationEvent>>>()))
            .Do(info =>
            {
                _publishedIntegrationEvent = info.Arg<PunchItemRejectedIntegrationEvent>();
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemRejectedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemRejectedEvent, default);

        // Assert
        await _publishEndpointMock.Received()
            .Publish(Arg.Any<PunchItemRejectedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemRejectedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemRejectedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item rejected", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemRejectedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemRejectedEvent.PunchItem.RejectedAtUtc, _publishedIntegrationEvent.RejectedAtUtc);
        Assert.AreEqual(_person.Guid, _publishedIntegrationEvent.RejectedByOid);
        Assert.AreEqual(_punchItemRejectedEvent.PunchItem.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_punchItemRejectedEvent.PunchItem.ModifiedBy!.Guid, _publishedIntegrationEvent.ModifiedByOid);
    }
}
