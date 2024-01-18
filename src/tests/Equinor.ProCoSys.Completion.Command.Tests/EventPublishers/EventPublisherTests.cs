using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventPublishers;

[TestClass]
public class EventPublisherTests
{
    [TestMethod]
    public async Task Publish_ShouldPublishToEndpoint()
    {
        // Arrange
        var publishEndpointMock = Substitute.For<IPublishEndpoint>();
        var dut = new EventPublisher(publishEndpointMock, Substitute.For<ILogger<EventPublisher>>());
        var message = new TestMessage(Guid.NewGuid());

        // Act
        await dut.PublishAsync(message, default);

        // Assert
        await publishEndpointMock.Received(1)
            .Publish(message, Arg.Any<IPipe<PublishContext<PunchItemCreatedIntegrationEvent>>>());
    }

    record TestMessage(Guid Guid) : IIntegrationEvent;
}
