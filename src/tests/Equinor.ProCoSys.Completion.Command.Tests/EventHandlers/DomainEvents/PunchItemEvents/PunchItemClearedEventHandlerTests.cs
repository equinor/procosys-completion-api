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
public class PunchItemClearedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemClearedEventHandler _dut;
    private PunchItemClearedDomainEvent _domainEvent;
    private IPublishEndpoint _publishEndpointMock;
    private PunchItemUpdatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.Clear(_person);
        _punchItem.SetModified(_person);

        _domainEvent = new PunchItemClearedDomainEvent(_punchItem);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new PunchItemClearedEventHandler(_publishEndpointMock, Substitute.For<ILogger<PunchItemClearedEventHandler>>());
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
        await _dut.Handle(_domainEvent, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(Arg.Any<PunchItemUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemUpdatedIntegrationEvent>>>());
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent_WithRequiredPropertiesSet()
    {
        // Act
        await _dut.Handle(_domainEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item cleared", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_domainEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        AssertModified(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertRequiredProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertOptionalPropertiesIsNull(_publishedIntegrationEvent);
        AssertIsCleared(_domainEvent.PunchItem, _person, _publishedIntegrationEvent);
        AssertNotRejected(_publishedIntegrationEvent);
        AssertNotVerified(_publishedIntegrationEvent);
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent_WithAllPropertiesSet()
    {
        // Arrange
        FillOptionalProperties(_domainEvent.PunchItem);

        // Act
        await _dut.Handle(_domainEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item cleared", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_domainEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        AssertModified(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertRequiredProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertOptionalProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertIsCleared(_domainEvent.PunchItem, _person, _publishedIntegrationEvent);
        AssertNotRejected(_publishedIntegrationEvent);
        AssertNotVerified(_publishedIntegrationEvent);
    }
}
