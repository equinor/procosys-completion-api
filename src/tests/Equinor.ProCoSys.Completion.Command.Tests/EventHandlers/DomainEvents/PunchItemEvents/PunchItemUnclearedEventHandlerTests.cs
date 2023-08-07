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
public class PunchItemUnclearedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemUnclearedEventHandler _dut;
    private PunchItemUnclearedDomainEvent _punchItemUnclearedEvent;
    private Mock<IPublishEndpoint> _publishEndpointMock;
    private PunchItemUnclearedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.Clear(_person);
        _punchItem.Unclear();
        _punchItem.SetModified(_person);

        _punchItemUnclearedEvent = new PunchItemUnclearedDomainEvent(_punchItem);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _dut = new PunchItemUnclearedEventHandler(_publishEndpointMock.Object, new Mock<ILogger<PunchItemUnclearedEventHandler>>().Object);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<PunchItemUnclearedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemUnclearedIntegrationEvent>>>(), default))
            .Callback<PunchItemUnclearedIntegrationEvent, IPipe<PublishContext<PunchItemUnclearedIntegrationEvent>>,
                CancellationToken>((evt, _, _) =>
            {
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemUnclearedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemUnclearedEvent, default);

        // Assert
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PunchItemUnclearedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemUnclearedIntegrationEvent>>>(), default), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemUnclearedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item uncleared", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemUnclearedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemUnclearedEvent.PunchItem.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_punchItemUnclearedEvent.PunchItem.ModifiedByOid, _publishedIntegrationEvent.ModifiedByOid);
    }
}
