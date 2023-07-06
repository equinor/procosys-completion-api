using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

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
        var punchItem = new PunchItem("X", new Project("X", Guid.NewGuid(), "Pro", "Desc"), "F");
        punchItem.Clear(_person);
        punchItem.Verify(_person);
        punchItem.SetModified(_person);

        _punchItemVerifiedEvent = new PunchItemVerifiedDomainEvent(punchItem, _person.Guid);
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
