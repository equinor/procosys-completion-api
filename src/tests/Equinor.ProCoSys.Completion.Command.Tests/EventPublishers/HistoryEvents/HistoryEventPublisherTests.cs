using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventPublishers.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventPublishers.HistoryEvents;

[TestClass]
public class HistoryEventPublisherTests
{
    protected string _plant = "X";
    protected string _displayName = "D";
    protected Guid _guid = Guid.NewGuid();
    protected Guid _parentGuid = Guid.NewGuid();
    protected User _user = new(Guid.NewGuid(), "N");
    protected DateTime _dateTime = DateTime.Now;
    private IPublishEndpoint _publishEndpointMock;
    private HistoryEventPublisher _dut;
    private readonly List<IProperty> _properties = [];
    private readonly List<IChangedProperty> _changedProperties = [];

    [TestInitialize]
    public void Setup()
    {
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new HistoryEventPublisher(_publishEndpointMock);
    }

    [TestMethod]
    public async Task PublishCreatedEvent_ShouldPublish_HistoryCreatedIntegrationEvent()
    {
        // Arrange
        HistoryCreatedIntegrationEvent integrationEvent = null;
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<HistoryCreatedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<HistoryCreatedIntegrationEvent>>>()))
            .Do(info => integrationEvent = info.Arg<HistoryCreatedIntegrationEvent>());

        // Act
        await _dut.PublishCreatedEventAsync(_plant, _displayName, _guid, _parentGuid, _user, _dateTime, _properties, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(
                Arg.Any<HistoryCreatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<HistoryCreatedIntegrationEvent>>>());
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(_plant, integrationEvent.Plant);
        Assert.AreEqual(_displayName, integrationEvent.DisplayName);
        Assert.AreEqual(_guid, integrationEvent.Guid);
        Assert.AreEqual(_parentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_user, integrationEvent.EventBy);
        Assert.AreEqual(_dateTime, integrationEvent.EventAtUtc);
        Assert.AreEqual(_properties, integrationEvent.Properties);
    }

    [TestMethod]
    public async Task PublishUpdatedEvent_ShouldPublish_HistoryUpdatedIntegrationEvent()
    {
        // Arrange
        HistoryUpdatedIntegrationEvent integrationEvent = null;
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<HistoryUpdatedIntegrationEvent>>>()))
            .Do(info => integrationEvent = info.Arg<HistoryUpdatedIntegrationEvent>());

        // Act
        await _dut.PublishUpdatedEventAsync(_plant, _displayName, _guid, _user, _dateTime, _changedProperties, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(
                Arg.Any<HistoryUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<HistoryUpdatedIntegrationEvent>>>());
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(_plant, integrationEvent.Plant);
        Assert.AreEqual(_displayName, integrationEvent.DisplayName);
        Assert.AreEqual(_guid, integrationEvent.Guid);
        Assert.AreEqual(_user, integrationEvent.EventBy);
        Assert.AreEqual(_dateTime, integrationEvent.EventAtUtc);
        Assert.AreEqual(_changedProperties, integrationEvent.ChangedProperties);
    }

    [TestMethod]
    public async Task PublishDeletedEvent_ShouldPublish_HistoryDeletedIntegrationEvent()
    {
        // Arrange
        HistoryDeletedIntegrationEvent integrationEvent = null!;
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<HistoryDeletedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<HistoryDeletedIntegrationEvent>>>()))
            .Do(info => integrationEvent = info.Arg<HistoryDeletedIntegrationEvent>());

        // Act
        await _dut.PublishDeletedEventAsync(_plant, _displayName, _guid, _parentGuid, _user, _dateTime, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(
                Arg.Any<HistoryDeletedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<HistoryDeletedIntegrationEvent>>>());
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(_plant, integrationEvent.Plant);
        Assert.AreEqual(_displayName, integrationEvent.DisplayName);
        Assert.AreEqual(_guid, integrationEvent.Guid);
        Assert.AreEqual(_parentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_user, integrationEvent.EventBy);
        Assert.AreEqual(_dateTime, integrationEvent.EventAtUtc);
    }
}
