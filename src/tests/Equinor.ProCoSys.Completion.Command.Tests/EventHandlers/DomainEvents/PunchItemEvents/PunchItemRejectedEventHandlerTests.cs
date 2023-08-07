using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.PunchItemEvents;

[TestClass]
public class PunchItemRejectedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemRejectedEventHandler _dut;
    private PunchItemRejectedDomainEvent _punchItemRejectedEvent;
    private Mock<IPublishEndpoint> _publishEndpointMock;
    private PunchItemRejectedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.Clear(_person);
        _punchItem.Reject(_person);
        _punchItem.SetModified(_person);

        _punchItemRejectedEvent = new PunchItemRejectedDomainEvent(_punchItem, _person.Guid);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _dut = new PunchItemRejectedEventHandler(_publishEndpointMock.Object, new Mock<ILogger<PunchItemRejectedEventHandler>>().Object);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<PunchItemRejectedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemRejectedIntegrationEvent>>>(), default))
            .Callback<PunchItemRejectedIntegrationEvent, IPipe<PublishContext<PunchItemRejectedIntegrationEvent>>,
                CancellationToken>((evt, _, _) =>
            {
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemRejectedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemRejectedEvent, default);

        // Assert
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PunchItemRejectedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemRejectedIntegrationEvent>>>(), default), Times.Once);
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
        Assert.AreEqual(_punchItemRejectedEvent.PunchItem.ModifiedByOid, _publishedIntegrationEvent.ModifiedByOid);
    }
}
