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
    private readonly IUserPropertyHelper _propertyHelperMock = Substitute.For<IUserPropertyHelper>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private HistoryItemUpdatedEventConsumer _dut = null!;
    private readonly ConsumeContext<IHistoryItemUpdatedV1> _contextMock = Substitute.For<ConsumeContext<IHistoryItemUpdatedV1>>();
    private HistoryItem? _historyItemAddedToRepository;
    private HistoryUpdatedIntegrationEvent _historyUpdatedIntegrationEvent = null!;

    [TestInitialize]
    public void Setup()
    {
        _historyUpdatedIntegrationEvent = new(
            "D",
            Guid.NewGuid(),
            Guid.NewGuid(),
            new User(Guid.NewGuid(), "Yoda"),
            DateTime.UtcNow,
            []);
        _dut = new HistoryItemUpdatedEventConsumer(
            Substitute.For<ILogger<HistoryItemUpdatedEventConsumer>>(),
            _propertyHelperMock,
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
        Assert.AreEqual(0, _historyItemAddedToRepository.Properties.Count);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_WithProperty()
    {
        // Arrange
        var propertyInEvent = new ChangedProperty<object>("P", 1, 2);
        _historyUpdatedIntegrationEvent.ChangedProperties.Add(propertyInEvent);

        // Act
        await _dut.Consume(_contextMock);

        // Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.IsNotNull(_historyItemAddedToRepository.Properties);
        var property = _historyItemAddedToRepository.Properties.ElementAt(0);
        Assert.AreEqual(propertyInEvent.Name, property.Name);
        Assert.AreEqual(propertyInEvent.ValueDisplayType.ToString(), property.ValueDisplayType);

        Assert.AreEqual(propertyInEvent.Value!.ToString(), property.Value);
        Assert.AreEqual(propertyInEvent.OldValue!.ToString(), property.OldValue);
        Assert.IsNull(property.OidValue);
        Assert.IsNull(property.OldOidValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_WithUserProperty()
    {
        // Arrange
        var propertyInEvent = new ChangedProperty<object>("P", "{oldjson}", "{json}", ValueDisplayType.UserAsNameOnly);
        _historyUpdatedIntegrationEvent.ChangedProperties.Add(propertyInEvent);
        var oldUserValue = new User(Guid.NewGuid(), "Yoda");
        var userValue = new User(Guid.NewGuid(), "Grogu");
        _propertyHelperMock.GetPropertyValueAsUser(propertyInEvent.Value, propertyInEvent.ValueDisplayType)
            .Returns(userValue);
        _propertyHelperMock.GetPropertyValueAsUser(propertyInEvent.OldValue, propertyInEvent.ValueDisplayType)
            .Returns(oldUserValue);

        // Act
        await _dut.Consume(_contextMock);

        // Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.IsNotNull(_historyItemAddedToRepository.Properties);
        var property = _historyItemAddedToRepository.Properties.ElementAt(0);
        Assert.AreEqual(propertyInEvent.Name, property.Name);
        Assert.AreEqual(propertyInEvent.ValueDisplayType.ToString(), property.ValueDisplayType);

        Assert.AreEqual(userValue.FullName, property.Value);
        Assert.AreEqual(userValue.Oid, property.OidValue);
        Assert.AreEqual(oldUserValue.FullName, property.OldValue);
        Assert.AreEqual(oldUserValue.Oid, property.OldOidValue);
    }

    [TestMethod]
    public async Task Consume_ShouldSave()
    {
        // Act
        await _dut.Consume(_contextMock);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
}
