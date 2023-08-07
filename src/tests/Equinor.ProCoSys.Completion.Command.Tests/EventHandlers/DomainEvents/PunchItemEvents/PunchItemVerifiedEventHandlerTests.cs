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
public class PunchItemVerifiedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemVerifiedEventHandler _dut;
    private PunchItemVerifiedDomainEvent _punchItemVerifiedEvent;
    private Mock<IPublishEndpoint> _publishEndpointMock;
    private PunchItemVerifiedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.Clear(_person);
        _punchItem.Verify(_person);
        _punchItem.SetModified(_person);

        _punchItemVerifiedEvent = new PunchItemVerifiedDomainEvent(_punchItem, _person.Guid);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _dut = new PunchItemVerifiedEventHandler(_publishEndpointMock.Object, new Mock<ILogger<PunchItemVerifiedEventHandler>>().Object);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<PunchItemVerifiedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemVerifiedIntegrationEvent>>>(), default))
            .Callback<PunchItemVerifiedIntegrationEvent, IPipe<PublishContext<PunchItemVerifiedIntegrationEvent>>,
                CancellationToken>((evt, _, _) =>
            {
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemVerifiedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemVerifiedEvent, default);

        // Assert
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PunchItemVerifiedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemVerifiedIntegrationEvent>>>(), default), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemVerifiedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item verified", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemVerifiedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemVerifiedEvent.PunchItem.VerifiedAtUtc, _publishedIntegrationEvent.VerifiedAtUtc);
        Assert.AreEqual(_person.Guid, _publishedIntegrationEvent.VerifiedByOid);
        Assert.AreEqual(_punchItemVerifiedEvent.PunchItem.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_punchItemVerifiedEvent.PunchItem.ModifiedByOid, _publishedIntegrationEvent.ModifiedByOid);
    }
}
