using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.PunchItemEvents;

[TestClass]
public class PunchItemUpdatedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemUpdatedEventHandler _dut;
    private PunchItemUpdatedDomainEvent _punchItemUpdatedEvent;
    private IPublishEndpoint _publishEndpointMock;
    private PunchItemUpdatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.SetModified(_person);

        _punchItemUpdatedEvent = new PunchItemUpdatedDomainEvent(_punchItem, new List<IProperty>());
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new PunchItemUpdatedEventHandler(_publishEndpointMock, Substitute.For<ILogger<PunchItemUpdatedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<PunchItemUpdatedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<PunchItemUpdatedIntegrationEvent>>>()))
            .Do(info =>
            {
                var evt = info.Arg<PunchItemUpdatedIntegrationEvent>();
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemUpdatedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemUpdatedEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
         .Publish(Arg.Any<PunchItemUpdatedIntegrationEvent>(),
             Arg.Any<IPipe<PublishContext<PunchItemUpdatedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemUpdatedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item updated", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemUpdatedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemUpdatedEvent.PunchItem.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_punchItemUpdatedEvent.PunchItem.ModifiedBy!.Guid, _publishedIntegrationEvent.ModifiedByOid);
        Assert.AreEqual(_punchItemUpdatedEvent.Changes, _publishedIntegrationEvent.Changes);
    }
}
