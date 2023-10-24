using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.PunchItemEvents;

[TestClass]
public class PunchItemCategoryUpdatedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemCategoryUpdatedEventHandler _dut;
    private PunchItemCategoryUpdatedDomainEvent _domainEvent;
    private IPublishEndpoint _publishEndpointMock;
    private PunchItemUpdatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        _punchItem.SetModified(_person);

        _domainEvent = new PunchItemCategoryUpdatedDomainEvent(_punchItem, new Property<string>("A", "1", "2"));
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new PunchItemCategoryUpdatedEventHandler(_publishEndpointMock, Substitute.For<ILogger<PunchItemCategoryUpdatedEventHandler>>());
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
        Assert.AreEqual($"Punch item category changed to {_domainEvent.PunchItem.Category}", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_domainEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(1, _publishedIntegrationEvent.Changes.Count);
        Assert.AreEqual(_domainEvent.Change, _publishedIntegrationEvent.Changes.ElementAt(0));
        AssertModified(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertRequiredProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertOptionalPropertiesIsNull(_publishedIntegrationEvent);
        AssertNotCleared(_publishedIntegrationEvent);
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
        Assert.AreEqual($"Punch item category changed to {_domainEvent.PunchItem.Category}", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_domainEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(1, _publishedIntegrationEvent.Changes.Count);
        Assert.AreEqual(_domainEvent.Change, _publishedIntegrationEvent.Changes.ElementAt(0));
        AssertModified(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertRequiredProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertOptionalProperties(_domainEvent.PunchItem, _publishedIntegrationEvent);
        AssertNotCleared(_publishedIntegrationEvent);
        AssertNotRejected(_publishedIntegrationEvent);
        AssertNotVerified(_publishedIntegrationEvent);
    }
}
