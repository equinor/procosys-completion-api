using System;
using System.Linq;
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
public class HistoryItemUpdatedEventConsumerTests
{
    private readonly IHistoryItemRepository _historyItemRepositoryMock = Substitute.For<IHistoryItemRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private HistoryItemUpdatedEventConsumer _dut = null!;
    private readonly ConsumeContext<IHistoryItemUpdatedV1> _contextMock = Substitute.For<ConsumeContext<IHistoryItemUpdatedV1>>();
    private HistoryItem? _historyItemAddedToRepository;
    private static readonly IChangedProperty s_propertyInEvent = new ChangedProperty<object>("P", 1, 2);
    private readonly HistoryUpdatedIntegrationEvent _historyUpdatedIntegrationEvent = new(
        "D",
        Guid.NewGuid(),
        Guid.NewGuid(),
        new User(Guid.NewGuid(), "Yoda"),
        DateTime.UtcNow,
        [s_propertyInEvent]);

    [TestInitialize]
    public void Setup()
    {
        _dut = new HistoryItemUpdatedEventConsumer(
            Substitute.For<ILogger<HistoryItemUpdatedEventConsumer>>(),
            _historyItemRepositoryMock,
            _unitOfWorkMock);
        _contextMock.Message.Returns(_historyUpdatedIntegrationEvent);
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
        Assert.AreEqual(_historyUpdatedIntegrationEvent.Guid, _historyItemAddedToRepository.EventForGuid);
        Assert.AreEqual(_historyUpdatedIntegrationEvent.ParentGuid, _historyItemAddedToRepository.EventForParentGuid);
        Assert.AreEqual(_historyUpdatedIntegrationEvent.EventAtUtc, _historyItemAddedToRepository.EventAtUtc);
        Assert.AreEqual(_historyUpdatedIntegrationEvent.EventBy.FullName, _historyItemAddedToRepository.EventByFullName);
        Assert.AreEqual(_historyUpdatedIntegrationEvent.EventBy.Oid, _historyItemAddedToRepository.EventByOid);

        Assert.IsNotNull(_historyItemAddedToRepository.Properties);
        var property = _historyItemAddedToRepository.Properties.ElementAt(0);
        Assert.AreEqual(s_propertyInEvent.Name, property.Name);
        Assert.AreEqual(s_propertyInEvent.Value!.ToString(), property.Value);
        Assert.AreEqual(s_propertyInEvent.OldValue!.ToString(), property.OldValue);
        Assert.AreEqual(s_propertyInEvent.ValueDisplayType.ToString(), property.ValueDisplayType);

        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
}
