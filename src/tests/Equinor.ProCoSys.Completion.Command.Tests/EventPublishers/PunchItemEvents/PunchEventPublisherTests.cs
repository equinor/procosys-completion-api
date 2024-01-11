using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Command.EventPublishers.PunchItemEvents;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventPublishers.PunchItemEvents;

[TestClass]
public class PunchEventPublisherTests
{
    private IPublishEndpoint _publishEndpointMock;
    private PunchEventPublisher _dut;
    protected Person _person;
    protected PunchItem _punchItem;

    [TestInitialize]
    public void Setup()
    {
        var plant = "X";
        TimeService.SetProvider(new ManualTimeProvider(new(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
        _person = new Person(Guid.NewGuid(), "Yo", "Da", "YD", "@", false);

        var project = new Project(plant, Guid.NewGuid(), null!, null!, DateTime.Now);
        var raisedByOrg = new LibraryItem(plant, Guid.NewGuid(), "RC", "RD", LibraryType.COMPLETION_ORGANIZATION);
        var clearingByOrg = new LibraryItem(plant, Guid.NewGuid(), "CC", "CD", LibraryType.COMPLETION_ORGANIZATION);
        _punchItem = new PunchItem(plant, project, Guid.NewGuid(), Category.PB, "Desc", raisedByOrg, clearingByOrg);
        _punchItem.SetCreated(_person);

        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _dut = new PunchEventPublisher(_publishEndpointMock);
    }

    [TestMethod]
    public async Task PublishCreatedEvent_ShouldPublish_PunchItemCreatedIntegrationEvent()
    {
        // Arrange
        PunchItemCreatedIntegrationEvent integrationEvent = null;
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<PunchItemCreatedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<PunchItemCreatedIntegrationEvent>>>()))
            .Do(info => integrationEvent = info.Arg<PunchItemCreatedIntegrationEvent>());

        // Act
        await _dut.PublishCreatedEventAsync(_punchItem, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(
                Arg.Any<PunchItemCreatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemCreatedIntegrationEvent>>>());
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
    }

    [TestMethod]
    public async Task PublishUpdatedEvent_ShouldPublish_PunchItemUpdatedIntegrationEvent()
    {
        // Arrange
        PunchItemUpdatedIntegrationEvent integrationEvent = null;
        _punchItem.SetModified(_person);
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<PunchItemUpdatedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<PunchItemUpdatedIntegrationEvent>>>()))
            .Do(info => integrationEvent = info.Arg<PunchItemUpdatedIntegrationEvent>());

        // Act
        await _dut.PublishUpdatedEventAsync(_punchItem, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(
                Arg.Any<PunchItemUpdatedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemUpdatedIntegrationEvent>>>());
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
    }

    [TestMethod]
    public async Task PublishDeletedEvent_ShouldPublish_PunchItemDeletedIntegrationEvent()
    {
        // Arrange
        PunchItemDeletedIntegrationEvent integrationEvent = null!;
        _punchItem.SetModified(_person);
        _publishEndpointMock
            .When(x => x.Publish(Arg.Any<PunchItemDeletedIntegrationEvent>(), Arg.Any<IPipe<PublishContext<PunchItemDeletedIntegrationEvent>>>()))
            .Do(info => integrationEvent = info.Arg<PunchItemDeletedIntegrationEvent>());

        // Act
        await _dut.PublishDeletedEventAsync(_punchItem, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(
                Arg.Any<PunchItemDeletedIntegrationEvent>(),
                Arg.Any<IPipe<PublishContext<PunchItemDeletedIntegrationEvent>>>());
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(_punchItem.Guid, integrationEvent.Guid);
    }
}
