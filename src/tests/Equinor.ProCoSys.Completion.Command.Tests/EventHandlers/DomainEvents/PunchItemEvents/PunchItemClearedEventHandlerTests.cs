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
public class PunchItemClearedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemClearedEventHandler _dut;
    private PunchItemClearedDomainEvent _punchItemClearedEvent;
    private Mock<IPublishEndpoint> _publishEndpointMock;
    private PunchItemClearedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        var punchItem = new PunchItem("X", new Project("X", Guid.NewGuid(), "Pro", "Desc"), "F");
        punchItem.Clear(_person);
        punchItem.SetModified(_person);

        _punchItemClearedEvent = new PunchItemClearedDomainEvent(punchItem, _person.Guid);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _dut = new PunchItemClearedEventHandler(_publishEndpointMock.Object, new Mock<ILogger<PunchItemClearedEventHandler>>().Object);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<PunchItemClearedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemClearedIntegrationEvent>>>(), default))
            .Callback<PunchItemClearedIntegrationEvent, IPipe<PublishContext<PunchItemClearedIntegrationEvent>>,
                CancellationToken>((evt, _, _) =>
            {
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemClearedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemClearedEvent, default);

        // Assert
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PunchItemClearedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemClearedIntegrationEvent>>>(), default), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemClearedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item cleared", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemClearedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemClearedEvent.PunchItem.ClearedAtUtc, _publishedIntegrationEvent.ClearedAtUtc);
        Assert.AreEqual(_person.Guid, _publishedIntegrationEvent.ClearedByOid);
        Assert.AreEqual(_punchItemClearedEvent.PunchItem.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_punchItemClearedEvent.PunchItem.ModifiedByOid, _publishedIntegrationEvent.ModifiedByOid);
    }
}
