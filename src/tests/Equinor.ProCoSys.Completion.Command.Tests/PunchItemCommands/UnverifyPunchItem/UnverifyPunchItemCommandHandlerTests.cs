using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UnverifyPunchItem;

[TestClass]
public class UnverifyPunchItemCommandHandlerTests : TestsBase
{
    private readonly int _currentPersonId = 13;
    private readonly string _rowVersion = "AAAAAAAAABA=";

    private Mock<IPunchItemRepository> _punchItemRepositoryMock;
    private PunchItem _existingPunchItem;

    private UnverifyPunchItemCommand _command;
    private UnverifyPunchItemCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        var person = new Person(Guid.NewGuid(), "F", "L", "UN", "@");
        person.SetProtectedIdForTesting(_currentPersonId);

        var project = new Project(TestPlantA, Guid.NewGuid(), "P", "D");
        _existingPunchItem = new PunchItem(TestPlantA, project, "X1");
        _existingPunchItem.Clear(person);
        _existingPunchItem.Verify(person);
        _punchItemRepositoryMock = new Mock<IPunchItemRepository>();
        _punchItemRepositoryMock.Setup(r => r.GetByGuidAsync(_existingPunchItem.Guid))
            .ReturnsAsync(_existingPunchItem);

        _command = new UnverifyPunchItemCommand(_existingPunchItem.Guid, _rowVersion);

        _dut = new UnverifyPunchItemCommandHandler(
            _punchItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new Mock<ILogger<UnverifyPunchItemCommandHandler>>().Object);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUnverifyPunchItem()
    {
        // Assert
        Assert.IsTrue(_existingPunchItem.VerifiedAtUtc.HasValue);
        Assert.IsTrue(_existingPunchItem.VerifiedById.HasValue);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsFalse(_existingPunchItem.VerifiedAtUtc.HasValue);
        Assert.IsFalse(_existingPunchItem.VerifiedById.HasValue);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _unitOfWorkMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_rowVersion, result.Data);
        Assert.AreEqual(_rowVersion, _existingPunchItem.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPunchItemUnverifiedDomainEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_existingPunchItem.DomainEvents.Last(), typeof(PunchItemUnverifiedDomainEvent));
    }
}
