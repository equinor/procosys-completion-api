using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.LinkEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Links;

[TestClass]
public class LinkServiceTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _parentGuid = Guid.NewGuid();
    private ILinkRepository _linkRepositoryMock;
    private IIntegrationEventPublisher _integrationEventPublisherMock;
    private LinkService _dut;
    private Link _linkAddedToRepository;
    private Link _existingLink;

    [TestInitialize]
    public void Setup()
    {
        _linkRepositoryMock = Substitute.For<ILinkRepository>();
        _linkRepositoryMock
            .When(x=> x.Add(Arg.Any<Link>()))
            .Do(info =>
            {
                _linkAddedToRepository = info.Arg<Link>();
                _linkAddedToRepository.SetCreated(_person);
            });
        _existingLink = new Link("Whatever", _parentGuid, "T", "www");
        _existingLink.SetCreated(_person);
        _existingLink.SetModified(_person);
        _linkRepositoryMock.GetAsync(_existingLink.Guid, default)
            .Returns(_existingLink);

        _integrationEventPublisherMock = Substitute.For<IIntegrationEventPublisher>();

        _dut = new LinkService(
            _linkRepositoryMock,
            _plantProviderMock,
            _unitOfWorkMock,
            _integrationEventPublisherMock,
            Substitute.For<ILogger<LinkService>>());
    }

    #region AddAsync
    [TestMethod]
    public async Task AddAsync_ShouldAddLinkToRepository()
    {
        // Arrange 
        var parentType = "Whatever";
        var title = "T";
        var url = "U";

        // Act
        await _dut.AddAsync(parentType, _parentGuid, title, url, default);

        // Assert
        Assert.IsNotNull(_linkAddedToRepository);
        Assert.AreEqual(_parentGuid, _linkAddedToRepository.ParentGuid);
        Assert.AreEqual(parentType, _linkAddedToRepository.ParentType);
        Assert.AreEqual(title, _linkAddedToRepository.Title);
        Assert.AreEqual(url, _linkAddedToRepository.Url);
    }

    [TestMethod]
    public async Task AddAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", "www", default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    [TestMethod]
    public async Task AddAsync_ShouldSetAuditDataAsyncOnce()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", "www", default);

        // Assert
        await _unitOfWorkMock.Received(1).SetAuditDataAsync();
    }

    [TestMethod]
    public async Task AddAsync_ShouldPublishLinkCreatedIntegrationEvent()
    {
        // Arrange
        LinkCreatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<LinkCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<LinkCreatedIntegrationEvent>();
            }));

        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", "www", default);

        // Assert
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(TestPlantA, integrationEvent.Plant);
        Assert.AreEqual(_linkAddedToRepository.Guid, integrationEvent.Guid);
        Assert.AreEqual(_linkAddedToRepository.ParentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_linkAddedToRepository.ParentType, integrationEvent.ParentType);
        Assert.AreEqual(_linkAddedToRepository.Title, integrationEvent.Title);
        Assert.AreEqual(_linkAddedToRepository.Url, integrationEvent.Url);
        Assert.AreEqual(_linkAddedToRepository.CreatedAtUtc, integrationEvent.CreatedAtUtc);
        Assert.AreEqual(_linkAddedToRepository.CreatedBy.Guid, integrationEvent.CreatedBy.Oid);
        Assert.AreEqual(_linkAddedToRepository.CreatedBy.GetFullName(), integrationEvent.CreatedBy.FullName);
    }

    [TestMethod]
    public async Task AddAsync_ShouldPublishHistoryCreatedIntegrationEvent()
    {
        // Arrange
        HistoryCreatedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryCreatedIntegrationEvent>();
            }));

        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", "www", default);

        // Assert
        AssertHistoryCreatedIntegrationEvent(
            historyEvent,
            _plantProviderMock.Plant,
            $"Link {_linkAddedToRepository.Title} created",
            _linkAddedToRepository.ParentGuid,
            _linkAddedToRepository,
            _linkAddedToRepository);

        Assert.AreEqual(2, historyEvent.Properties.Count);
        AssertProperty(
            historyEvent.Properties
                .SingleOrDefault(c => c.Name == nameof(Link.Title)),
            _linkAddedToRepository.Title);
        AssertProperty(
            historyEvent.Properties
                .SingleOrDefault(c => c.Name == nameof(Link.Url)),
            _linkAddedToRepository.Url);
    }
    #endregion

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenKnownLink()
    {
        // Arrange
        _linkRepositoryMock.ExistsAsync(_existingLink.Guid, default)
            .Returns(true);

        // Act
        var result = await _dut.ExistsAsync(_existingLink.Guid, default);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnNull_WhenUnknownLink()
    {
        // Act
        var result = await _dut.ExistsAsync(Guid.NewGuid(), default);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region UpdateAsync
    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateLink_WhenKnownLink()
    {
        // Arrange 
        var title = "newT";
        var url = "newU";

        // Act
        await _dut.UpdateAsync(_existingLink.Guid, title, url, _rowVersion, default);

        // Assert
        Assert.AreEqual(url, _existingLink.Url);
        Assert.AreEqual(title, _existingLink.Title);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSaveOnce_WhenNoChanges()
    {
        // Act
        await _dut.UpdateAsync(_existingLink.Guid, _existingLink.Title, _existingLink.Url, _rowVersion, default);

        // Assert
        await  _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSaveOnce_WhenChanges()
    {
        // Act
        await _dut.UpdateAsync(_existingLink.Guid, Guid.NewGuid().ToString(), _existingLink.Url, _rowVersion, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldNotPublishAnyIntegrationEvents_WhenNoChanges()
    {
        // Act
        await _dut.UpdateAsync(
            _existingLink.Guid,
            _existingLink.Title,
            _existingLink.Url,
            _rowVersion,
            default);

        // Assert
        await _integrationEventPublisherMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldPublishLinkUpdatedIntegrationEvent_WhenChanges()
    {
        // Arrange
        var newTitle = Guid.NewGuid().ToString();
        var newUrl = Guid.NewGuid().ToString();
        LinkUpdatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<LinkUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<LinkUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.UpdateAsync(
            _existingLink.Guid,
            newTitle,
            newUrl,
            _rowVersion,
            default);

        // Assert
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(TestPlantA, integrationEvent.Plant);
        Assert.AreEqual(_existingLink.Guid, integrationEvent.Guid);
        Assert.AreEqual(_existingLink.ParentGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(_existingLink.ParentType, integrationEvent.ParentType);
        Assert.AreEqual(_existingLink.Title, integrationEvent.Title);
        Assert.AreEqual(_existingLink.Url, integrationEvent.Url);
        Assert.AreEqual(_existingLink.ModifiedAtUtc, integrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_existingLink.ModifiedBy!.Guid, integrationEvent.ModifiedBy.Oid);
        Assert.AreEqual(_existingLink.ModifiedBy!.GetFullName(), integrationEvent.ModifiedBy.FullName);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldPublishHistoryUpdatedIntegrationEvent_WhenChanges()
    {
        // Arrange
        var oldTitle = _existingLink.Title;
        var oldUrl = _existingLink.Url;
        HistoryUpdatedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.UpdateAsync(
            _existingLink.Guid,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            _rowVersion,
            default);

        // Assert
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            _plantProviderMock.Plant,
            $"Link {_existingLink.Title} updated",
            _existingLink,
            _existingLink);
        Assert.AreEqual(2, historyEvent.ChangedProperties.Count);
        AssertChange(
            historyEvent.ChangedProperties
                .SingleOrDefault(c => c.Name == nameof(Link.Title)),
            oldTitle,
            _existingLink.Title);
        AssertChange(
            historyEvent.ChangedProperties
                .SingleOrDefault(c => c.Name == nameof(Link.Url)),
            oldUrl,
            _existingLink.Url);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSetAndReturnRowVersion()
    {
        // Act
        var result = await _dut.UpdateAsync(_existingLink.Guid, "T", "www", _rowVersion, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_rowVersion, result);
        Assert.AreEqual(_rowVersion, _existingLink.RowVersion.ConvertToString());
    }
    #endregion

    #region DeleteAsync
    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteLink_WhenKnownLink()
    {
        // Act
        await _dut.DeleteAsync(_existingLink.Guid, _rowVersion, default);

        // Assert
        _linkRepositoryMock.Received(1).Remove(_existingLink);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.DeleteAsync(_existingLink.Guid, _rowVersion, default);

        // Assert
        await  _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSetAndReturnRowVersion()
    {
        // Act
        await _dut.DeleteAsync(_existingLink.Guid, _rowVersion, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_rowVersion, _existingLink.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldPublishLinkDeletedIntegrationEvent()
    {
        // Arrange
        LinkDeletedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<LinkDeletedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<LinkDeletedIntegrationEvent>();
            }));

        // Act
        await _dut.DeleteAsync(_existingLink.Guid, _rowVersion, default);

        // Assert
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(TestPlantA, integrationEvent.Plant);
        Assert.AreEqual(_existingLink.Guid, integrationEvent.Guid);
        Assert.AreEqual(_existingLink.Guid, integrationEvent.Guid);
        Assert.AreEqual(_existingLink.ParentGuid, integrationEvent.ParentGuid);

        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... use ModifiedBy/ModifiedAtUtc which is set when saving a deletion
        Assert.AreEqual(_existingLink.ModifiedAtUtc, integrationEvent.DeletedAtUtc);
        Assert.AreEqual(_existingLink.ModifiedBy!.Guid, integrationEvent.DeletedBy.Oid);
        Assert.AreEqual(_existingLink.ModifiedBy!.GetFullName(), integrationEvent.DeletedBy.FullName);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldPublishHistoryDeletedIntegrationEvent()
    {
        // Arrange
        HistoryDeletedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryDeletedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryDeletedIntegrationEvent>();
            }));

        // Act
        await _dut.DeleteAsync(_existingLink.Guid, _rowVersion, default);

        // Assert
        AssertHistoryDeletedIntegrationEvent(
            historyEvent,
            _plantProviderMock.Plant,
            $"Link {_existingLink.Title} deleted",
            _existingLink.ParentGuid,
            _existingLink,
            _existingLink);
    }
    #endregion
}
