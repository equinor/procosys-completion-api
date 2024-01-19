using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts.History;
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
            });
        _existingLink = new Link("Whatever", _parentGuid, "T", "www");
        _linkRepositoryMock.GetAsync(_existingLink.Guid, default)
            .Returns(_existingLink);

        _dut = new LinkService(
            _linkRepositoryMock,
            _unitOfWorkMock,
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
    public async Task AddAsync_ShouldAddLinkCreatedEvent()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", "www", default);

        // Assert
        Assert.IsInstanceOfType(_linkAddedToRepository.DomainEvents.First(), typeof(LinkCreatedDomainEvent));
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
    public async Task UpdateAsync_ShouldNotAddLinkUpdatedEvent_WhenNoChanges()
    {
        // Act
        await _dut.UpdateAsync(_existingLink.Guid, _existingLink.Title, _existingLink.Url, _rowVersion, default);

        // Assert
        var linkUpdatedDomainEventAdded =
            _existingLink.DomainEvents.Any(e => e.GetType() == typeof(LinkUpdatedDomainEvent));
        Assert.IsFalse(linkUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldAddCorrectLinkUpdatedEvent_WhenChanges()
    {
        // Arrange
        var oldUrl = _existingLink.Url;
        var oldTitle = _existingLink.Title;
        var newUrl = Guid.NewGuid().ToString();
        var newTitle = Guid.NewGuid().ToString();

        // Act
        await _dut.UpdateAsync(_existingLink.Guid, newTitle, newUrl, _rowVersion, default);

        // Assert
        var linkUpdatedDomainEventAdded = _existingLink.DomainEvents.Last() as LinkUpdatedDomainEvent;
        Assert.IsNotNull(linkUpdatedDomainEventAdded);
        Assert.IsNotNull(linkUpdatedDomainEventAdded.Changes);
        AssertChange(
            linkUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(Link.Url)),
            oldUrl,
            newUrl);
        AssertChange(
            linkUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(Link.Title)),
            oldTitle,
            newTitle);
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
    public async Task DeleteAsync_ShouldAddLinkDeletedEvent()
    {
        // Act
        await _dut.DeleteAsync(_existingLink.Guid, _rowVersion, default);

        // Assert
        Assert.IsInstanceOfType(_existingLink.DomainEvents.Last(), typeof(LinkDeletedDomainEvent));
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
    #endregion

    private void AssertChange(IChangedProperty change, object oldValue, object newValue)
    {
        Assert.IsNotNull(change);
        Assert.AreEqual(oldValue, change.OldValue);
        Assert.AreEqual(newValue, change.NewValue);
    }
}
