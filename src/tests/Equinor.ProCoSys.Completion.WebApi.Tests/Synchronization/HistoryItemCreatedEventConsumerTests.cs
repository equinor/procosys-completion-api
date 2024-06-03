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
using Property = Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents.Property;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class HistoryItemCreatedEventConsumerTests
{
    private readonly IHistoryItemRepository _historyItemRepositoryMock = Substitute.For<IHistoryItemRepository>();
    private readonly IUserPropertyHelper _propertyHelperMock = Substitute.For<IUserPropertyHelper>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private HistoryItemCreatedEventConsumer _dut = null!;
    private readonly ConsumeContext<IHistoryItemCreatedV1> _contextMock = Substitute.For<ConsumeContext<IHistoryItemCreatedV1>>();
    private HistoryItem? _historyItemAddedToRepository;
    private static readonly IProperty s_propertyInEvent = new Property("P", 1);
    private readonly HistoryCreatedIntegrationEvent _historyCreatedIntegrationEvent = new(
        "D",
        Guid.NewGuid(),
        Guid.NewGuid(),
        new User(Guid.NewGuid(), "Yoda"),
        DateTime.UtcNow,
        [s_propertyInEvent]);

    [TestInitialize]
    public void Setup()
    {
        _dut = new HistoryItemCreatedEventConsumer(
            Substitute.For<ILogger<HistoryItemCreatedEventConsumer>>(),
            _propertyHelperMock,
            _historyItemRepositoryMock,
            _unitOfWorkMock);
        _contextMock.Message.Returns(_historyCreatedIntegrationEvent);
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
        Assert.AreEqual(_historyCreatedIntegrationEvent.Guid, _historyItemAddedToRepository.EventForGuid);
        Assert.AreEqual(_historyCreatedIntegrationEvent.ParentGuid, _historyItemAddedToRepository.EventForParentGuid);
        Assert.AreEqual(_historyCreatedIntegrationEvent.EventAtUtc, _historyItemAddedToRepository.EventAtUtc);
        Assert.AreEqual(_historyCreatedIntegrationEvent.EventBy.FullName, _historyItemAddedToRepository.EventByFullName);
        Assert.AreEqual(_historyCreatedIntegrationEvent.EventBy.Oid, _historyItemAddedToRepository.EventByOid);

        Assert.IsNotNull(_historyItemAddedToRepository.Properties);
        var property = _historyItemAddedToRepository.Properties.ElementAt(0);
        Assert.AreEqual(s_propertyInEvent.Name, property.Name);
        Assert.AreEqual(s_propertyInEvent.Value!.ToString(), property.Value);
        Assert.AreEqual(s_propertyInEvent.ValueDisplayType.ToString(), property.ValueDisplayType);

        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
}
