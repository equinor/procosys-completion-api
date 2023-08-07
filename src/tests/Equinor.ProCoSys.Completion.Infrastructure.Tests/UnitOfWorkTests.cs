using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests;

[TestClass]
public class UnitOfWorkTests
{
    private readonly string _plant = "PCS$TESTPLANT";
    private Project _project;
    private LibraryItem _raisedByOrg;
    private LibraryItem _clearingByOrg;
    private readonly Guid _currentUserOid = new("12345678-1234-1234-1234-123456789123");
    private readonly DateTime _currentTime = new(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    private DbContextOptions<CompletionContext> _dbContextOptions;
    private Mock<IPlantProvider> _plantProviderMock;
    private Mock<IEventDispatcher> _eventDispatcherMock;
    private Mock<ICurrentUserProvider> _currentUserProviderMock;
    private ManualTimeProvider _timeProvider;

    [TestInitialize]
    public void Setup()
    {
        _project = new(_plant, Guid.NewGuid(), null!, null!);
        _raisedByOrg = new LibraryItem(_plant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _clearingByOrg = new LibraryItem(_plant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);

        _dbContextOptions = new DbContextOptionsBuilder<CompletionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _plantProviderMock = new Mock<IPlantProvider>();
        _plantProviderMock.Setup(x => x.Plant)
            .Returns(_plant);

        _eventDispatcherMock = new Mock<IEventDispatcher>();

        _currentUserProviderMock = new Mock<ICurrentUserProvider>();

        _timeProvider = new ManualTimeProvider(_currentTime);
        TimeService.SetProvider(_timeProvider);
    }

    [TestMethod]
    public async Task SaveChangesAsync_SetsCreatedProperties_WhenCreated()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock.Object, _eventDispatcherMock.Object, _currentUserProviderMock.Object);

        var user = new Person(_currentUserOid, "Current", "User", "cu", "cu@pcs.pcs");
        dut.Persons.Add(user);
        await dut.SaveChangesAsync();

        _currentUserProviderMock
            .Setup(x => x.GetCurrentUserOid())
            .Returns(_currentUserOid);
        var newPunchItem = new PunchItem(_plant, _project, "Desc", _raisedByOrg, _clearingByOrg);
        dut.PunchItems.Add(newPunchItem);

        // Act
        await dut.SaveChangesAsync();

        // Assert
        Assert.AreEqual(_currentTime, newPunchItem.CreatedAtUtc);
        Assert.AreEqual(user.Id, newPunchItem.CreatedById);
        Assert.AreEqual(_currentUserOid, newPunchItem.CreatedByOid);
        Assert.IsNull(newPunchItem.ModifiedAtUtc);
        Assert.IsNull(newPunchItem.ModifiedById);
        Assert.IsNull(newPunchItem.ModifiedByOid);
    }

    [TestMethod]
    public async Task SaveChangesAsync_SetsModifiedProperties_WhenModified()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock.Object, _eventDispatcherMock.Object, _currentUserProviderMock.Object);

        var user = new Person(_currentUserOid, "Current", "User", "cu", "cu@pcs.pcs");
        dut.Persons.Add(user);
        await dut.SaveChangesAsync();

        _currentUserProviderMock
            .Setup(x => x.GetCurrentUserOid())
            .Returns(_currentUserOid);

        var newPunchItem = new PunchItem(_plant, _project, "Desc", _raisedByOrg, _clearingByOrg);
        dut.PunchItems.Add(newPunchItem);

        await dut.SaveChangesAsync();

        newPunchItem.Description = "Updated";
            
        // Act
        await dut.SaveChangesAsync();

        // Assert
        Assert.AreEqual(_currentTime, newPunchItem.ModifiedAtUtc);
        Assert.AreEqual(user.Id, newPunchItem.ModifiedById);
        Assert.AreEqual(_currentUserOid, newPunchItem.ModifiedByOid);
    }
}
