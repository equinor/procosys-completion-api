using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
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
    private Person _user;

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

        _user = new Person(_currentUserOid, "Current", "User", "cu", "cu@pcs.pcs");
        dut.Persons.Add(_user);
        await dut.SaveChangesAsync();

        _currentUserProviderMock.GetCurrentUserOid()
            .Returns(_currentUserOid);
    }

    [TestMethod]
    public async Task SaveChangesAsync_SetsCreatedProperties_WhenCreated()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        var raisedByOrg = new LibraryItem(_plant, Guid.NewGuid(), "EQ", "Equinor", LibraryType.COMPLETION_ORGANIZATION);
        dut.Library.Add(raisedByOrg);

        // Act
        await dut.SaveChangesAsync();

        // Assert
        Assert.AreEqual(_currentTime, raisedByOrg.CreatedAtUtc);
        Assert.AreEqual(_user.Id, raisedByOrg.CreatedById);
        Assert.AreEqual(_currentUserOid, raisedByOrg.CreatedByOid);
        Assert.IsNull(raisedByOrg.ModifiedAtUtc);
        Assert.IsNull(raisedByOrg.ModifiedById);
        Assert.IsNull(raisedByOrg.ModifiedByOid);
    }

    [TestMethod]
    public async Task SaveChangesAsync_SetsModifiedProperties_WhenModified()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        var raisedByOrg = new LibraryItem(_plant, Guid.NewGuid(), "EQ", "Equinor", LibraryType.COMPLETION_ORGANIZATION);
        dut.Library.Add(raisedByOrg);

        await dut.SaveChangesAsync();

        // trigger a change on record. EF change tracker notice this
        raisedByOrg.IsVoided = true;
            
        // Act
        await dut.SaveChangesAsync();

        // Assert
        Assert.AreEqual(_currentTime, raisedByOrg.ModifiedAtUtc);
        Assert.AreEqual(_user.Id, raisedByOrg.ModifiedById);
        Assert.AreEqual(_currentUserOid, raisedByOrg.ModifiedByOid);
    }
}
