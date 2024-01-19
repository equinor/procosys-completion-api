using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests;

[TestClass]
public class UnitOfWorkTests
{
    private readonly string _plant = "PCS$TESTPLANT";
    private readonly Guid _currentUserOid = new("12345678-1234-1234-1234-123456789123");
    private readonly DateTime _currentTime = new(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    private DbContextOptions<CompletionContext> _dbContextOptions;
    private IPlantProvider _plantProviderMock;
    private IEventDispatcher _eventDispatcherMock;
    private ICurrentUserProvider _currentUserProviderMock;
    private ManualTimeProvider _timeProvider;
    private Person _currentUser;

    [TestInitialize]
    public async Task Setup()
    {
        _dbContextOptions = new DbContextOptionsBuilder<CompletionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant
            .Returns(_plant);

        _eventDispatcherMock = Substitute.For<IEventDispatcher>();

        _currentUserProviderMock = Substitute.For<ICurrentUserProvider>();

        _timeProvider = new ManualTimeProvider(_currentTime);
        TimeService.SetProvider(_timeProvider);

        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        _currentUser = new Person(_currentUserOid, "Current", "User", "cu", "cu@pcs.pcs", false);
        dut.Persons.Add(_currentUser);
        await dut.SaveChangesAsync();

        _currentUserProviderMock.GetCurrentUserOid()
            .Returns(_currentUserOid);
    }

    [TestMethod]
    public async Task SetAuditDataAsync_ShouldSetCreatedProperties_WhenCreated()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        var libraryItem = new LibraryItem(_plant, Guid.NewGuid(), "EQ", "Equinor", LibraryType.COMPLETION_ORGANIZATION);
        dut.Library.Add(libraryItem);

        // Act
        await dut.SetAuditDataAsync();

        // Assert
        AssertCreated(libraryItem);
        AssertNotModified(libraryItem);
    }

    [TestMethod]
    public async Task SaveChangesAsync_ShouldSetCreatedProperties_WhenCreated()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        var libraryItem = new LibraryItem(_plant, Guid.NewGuid(), "EQ", "Equinor", LibraryType.COMPLETION_ORGANIZATION);
        dut.Library.Add(libraryItem);

        // Act
        await dut.SaveChangesAsync();

        // Assert
        AssertCreated(libraryItem);
        AssertNotModified(libraryItem);
    }

    [TestMethod]
    public async Task SetAuditDataAsync_ShouldSetModifiedProperties_WhenModified()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        var libraryItem = new LibraryItem(_plant, Guid.NewGuid(), "EQ", "Equinor", LibraryType.COMPLETION_ORGANIZATION);
        dut.Library.Add(libraryItem);

        await dut.SaveChangesAsync();

        // trigger a change on record. EF change tracker notice this
        libraryItem.IsVoided = true;

        // Act
        await dut.SetAuditDataAsync();

        // Assert
        AssertCreated(libraryItem);
        AssertModified(libraryItem);
    }

    [TestMethod]
    public async Task SaveChangesAsync_ShouldSetModifiedProperties_WhenModified()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        var libraryItem = new LibraryItem(_plant, Guid.NewGuid(), "EQ", "Equinor", LibraryType.COMPLETION_ORGANIZATION);
        dut.Library.Add(libraryItem);

        await dut.SaveChangesAsync();

        // trigger a change on record. EF change tracker notice this
        libraryItem.IsVoided = true;

        // Act
        await dut.SaveChangesAsync();

        // Assert
        AssertCreated(libraryItem);
        AssertModified(libraryItem);
    }

    private void AssertModified(IModificationAuditable auditable)
    {
        Assert.AreEqual(_currentTime, auditable.ModifiedAtUtc);
        Assert.AreEqual(_currentUser.Id, auditable.ModifiedById);
        Assert.AreEqual(_currentUser.Guid, auditable.ModifiedBy!.Guid);
    }

    private void AssertNotModified(IModificationAuditable auditable)
    {
        Assert.IsNull(auditable.ModifiedAtUtc);
        Assert.IsNull(auditable.ModifiedById);
        Assert.IsNull(auditable.ModifiedBy);
    }

    private void AssertCreated(ICreationAuditable auditable)
    {
        Assert.AreEqual(_currentTime, auditable.CreatedAtUtc);
        Assert.AreEqual(_currentUser.Id, auditable.CreatedById);
        Assert.AreEqual(_currentUser.Guid, auditable.CreatedBy.Guid);
    }
}
