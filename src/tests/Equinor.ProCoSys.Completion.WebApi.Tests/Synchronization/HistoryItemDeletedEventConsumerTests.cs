using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class HistoryItemDeletedEventConsumerTests
{
    private readonly IHistoryItemRepository _historyItemRepositoryMock = Substitute.For<IHistoryItemRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private HistoryItemDeletedEventConsumer _dut = null!;
    private readonly ConsumeContext<IHistoryItemDeletedV1> _contextMock = Substitute.For<ConsumeContext<IHistoryItemDeletedV1>>();
    private HistoryItem? _historyItemAddedToRepository;
    private readonly HistoryDeletedIntegrationEvent _historyDeletedIntegrationEvent = new(
        "D",
        Guid.NewGuid(),
        Guid.NewGuid(),
        new User(Guid.NewGuid(), "Yoda"),
        DateTime.UtcNow);

    [TestInitialize]
    public void Setup()
    {
        _dut = new HistoryItemDeletedEventConsumer(
            Substitute.For<ILogger<HistoryItemDeletedEventConsumer>>(),
            _historyItemRepositoryMock,
            _unitOfWorkMock);
        _contextMock.Message.Returns(_historyDeletedIntegrationEvent);
        _historyItemRepositoryMock
            .When(x => x.Add(Arg.Any<HistoryItem>()))
            .Do(callInfo =>
            {
                _historyItemAddedToRepository = callInfo.Arg<HistoryItem>();
            });
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem()
    {
        // Act
        await _dut.Consume(_contextMock);

        // Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual(_historyDeletedIntegrationEvent.Guid, _historyItemAddedToRepository.EventForGuid);
        Assert.AreEqual(_historyDeletedIntegrationEvent.ParentGuid, _historyItemAddedToRepository.EventForParentGuid);
        Assert.AreEqual(_historyDeletedIntegrationEvent.EventAtUtc, _historyItemAddedToRepository.EventAtUtc);
        Assert.AreEqual(_historyDeletedIntegrationEvent.EventBy.FullName, _historyItemAddedToRepository.EventByFullName);
        Assert.AreEqual(_historyDeletedIntegrationEvent.EventBy.Oid, _historyItemAddedToRepository.EventByOid);

        Assert.IsNotNull(_historyItemAddedToRepository.Properties);
        Assert.AreEqual(0, _historyItemAddedToRepository.Properties.Count);

        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
}
