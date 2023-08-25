using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Links;

[TestClass]
public class LinkServiceTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _sourceGuid = Guid.NewGuid();
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
        _existingLink = new Link("Whatever", _sourceGuid, "T", "www");
        _linkRepositoryMock.GetByGuidAsync(_existingLink.Guid)
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
        var sourceType = "Whatever";
        var title = "T";
        var url = "U";

        // Act
        await _dut.AddAsync(sourceType, _sourceGuid, title, url, default);

        // Assert
        Assert.IsNotNull(_linkAddedToRepository);
        Assert.AreEqual(_sourceGuid, _linkAddedToRepository.SourceGuid);
        Assert.AreEqual(sourceType, _linkAddedToRepository.SourceType);
        Assert.AreEqual(title, _linkAddedToRepository.Title);
        Assert.AreEqual(url, _linkAddedToRepository.Url);
    }

    [TestMethod]
    public async Task AddAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.AddAsync("Whatever", _sourceGuid, "T", "www", default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddLinkCreatedEvent()
    {
        // Act
        await _dut.AddAsync("Whatever", _sourceGuid, "T", "www", default);

        // Assert
        Assert.IsInstanceOfType(_linkAddedToRepository.DomainEvents.First(), typeof(LinkCreatedDomainEvent));
    }
    #endregion

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenKnownLink()
    {
        // Act
        var result = await _dut.ExistsAsync(_existingLink.Guid);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnNull_WhenUnknownLink()
    {
        // Act
        var result = await _dut.ExistsAsync(Guid.NewGuid());

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
    public async Task UpdateAsync_ShouldThrowException_WhenUnknownLink() =>
        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.UpdateAsync(Guid.NewGuid(), "T", "www", _rowVersion, default));

    [TestMethod]
    public async Task UpdateAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.UpdateAsync(_existingLink.Guid, "T", "www", _rowVersion, default);

        // Assert
        await  _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldAddLinkUpdatedEvent()
    {
        // Act
        await _dut.UpdateAsync(_existingLink.Guid, "T", "www", _rowVersion, default);

        // Assert
        Assert.IsInstanceOfType(_existingLink.DomainEvents.Last(), typeof(LinkUpdatedDomainEvent));
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
    public async Task DeleteAsync_ShouldThrowException_WhenUnknownLink() =>
        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.DeleteAsync(Guid.NewGuid(), _rowVersion, default));

    [TestMethod]
    public async Task DeleteAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.DeleteAsync(_existingLink.Guid, _rowVersion, default);

        // Assert
        await  _unitOfWorkMock.Received(1).SaveChangesAsync(default);
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
}
