using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.MessageProducers;

[TestClass]
public class MessageProducerTests
{
    private IPublishEndpoint _publishEndpointMock;
    private ISendEndpointProvider _sendEndpointProviderMock;
    private MessageProducer _dut;

    [TestInitialize]
    public void Setup()
    {
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _sendEndpointProviderMock = Substitute.For<ISendEndpointProvider>();
        _dut = new MessageProducer(_sendEndpointProviderMock, _publishEndpointMock, Substitute.For<ILogger<MessageProducer>>());
    }

    [TestMethod]
    public async Task Publish_ShouldPublishToEndpoint()
    {
        // Arrange
        var message = new TestMessage(Guid.NewGuid());

        // Act
        await _dut.PublishAsync(message, default);

        // Assert
        await _publishEndpointMock.Received(1)
            .Publish(
                message,
                Arg.Any<IPipe<PublishContext<TestMessage>>>());
    }

    [TestMethod]
    public async Task SendHistoryCreatedIntegrationEvent_ShouldSendToCreatedEndpoint()
    {
        // Arrange
        var history = new HistoryCreatedIntegrationEvent(
            "D",
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            new User(Guid.NewGuid(), "Per"), 
            DateTime.UtcNow, 
            new List<IProperty>());
        var endpoint = Substitute.For<ISendEndpoint>();
        _sendEndpointProviderMock.GetSendEndpoint(new Uri($"queue:{QueueNames.CompletionHistoryCreated}")).Returns(endpoint);

        // Act
        await _dut.SendHistoryAsync(history, default);

        // Assert
        await endpoint.Received(1).Send(history);
    }

    [TestMethod]
    public async Task SendHistoryDeletedIntegrationEvent_ShouldSendToDeletedEndpoint()
    {
        // Arrange
        var history = new HistoryDeletedIntegrationEvent(
            "D",
            Guid.NewGuid(),
            Guid.NewGuid(),
            new User(Guid.NewGuid(), "Per"),
            DateTime.UtcNow);
        var endpoint = Substitute.For<ISendEndpoint>();
        _sendEndpointProviderMock.GetSendEndpoint(new Uri($"queue:{QueueNames.CompletionHistoryDeleted}")).Returns(endpoint);

        // Act
        await _dut.SendHistoryAsync(history, default);

        // Assert
        await endpoint.Received(1).Send(history);
    }

    [TestMethod]
    public async Task SendHistoryUpdatedIntegrationEvent_ShouldSendToUpdatedEndpoint()
    {
        // Arrange
        var history = new HistoryUpdatedIntegrationEvent(
            "D",
            Guid.NewGuid(),
            Guid.NewGuid(),
            new User(Guid.NewGuid(), "Per"),
            DateTime.UtcNow,
            new List<IChangedProperty>());
        var endpoint = Substitute.For<ISendEndpoint>();
        _sendEndpointProviderMock.GetSendEndpoint(new Uri($"queue:{QueueNames.CompletionHistoryUpdated}")).Returns(endpoint);

        // Act
        await _dut.SendHistoryAsync(history, default);

        // Assert
        await endpoint.Received(1).Send(history);
    }

    [TestMethod]
    public async Task SendUnknownHistoryIntegrationEvent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var history = new TestHistoryMessage(
            "D",
            Guid.NewGuid(),
            new User(Guid.NewGuid(), "Per"),
            DateTime.UtcNow);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _dut.SendHistoryAsync(history, default));
    }

    record TestMessage(Guid Guid) : IIntegrationEvent;
    record TestHistoryMessage(string DisplayName, Guid? ParentGuid, User EventBy, DateTime EventAtUtc) : IHistoryItem;
}
