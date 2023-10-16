using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.PunchItemEvents;

[TestClass]
public class PunchItemVerifiedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemVerifiedEventHandler _dut;
    private PunchItemVerifiedDomainEvent _domainEvent;
    private IPublishEndpoint _publishEndpointMock;
    private PunchItemUpdatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.Clear(_person);
        _punchItem.Verify(_person);
        _punchItem.SetModified(_person);

        _domainEvent = new PunchItemVerifiedDomainEvent(_punchItem);
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new PunchItemVerifiedEventHandler(_publishEndpointMock, Substitute.For<ILogger<PunchItemVerifiedEventHandler>>());
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<PunchItemUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemUpdatedIntegrationEvent>>>()))
            .Do(info => 
            {
                _publishedIntegrationEvent = info.Arg<PunchItemUpdatedIntegrationEvent>();
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
        Assert.AreEqual("Punch item verified", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_domainEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_domainEvent.PunchItem.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_domainEvent.PunchItem.ModifiedBy!.Guid, _publishedIntegrationEvent.ModifiedByOid);
        AssertRequiredProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertOptionalPropertiesIsNull(_publishedIntegrationEvent);
        AssertIsCleared(_domainEvent.PunchItem, _person, _publishedIntegrationEvent);
        AssertNotRejected(_publishedIntegrationEvent);
        AssertIsVerified(_domainEvent.PunchItem, _person, _publishedIntegrationEvent);
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
        Assert.AreEqual("Punch item verified", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_domainEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        AssertRequiredProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertOptionalProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertIsCleared(_domainEvent.PunchItem, _person, _publishedIntegrationEvent);
        AssertNotRejected(_publishedIntegrationEvent);
        AssertIsVerified(_domainEvent.PunchItem, _person, _publishedIntegrationEvent);
    }
}
