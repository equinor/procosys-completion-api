using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItemCategory;

[TestClass]
public class UpdatePunchItemCategoryCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private UpdatePunchItemCategoryCommand _command;
    private UpdatePunchItemCategoryCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _command = new UpdatePunchItemCategoryCommand(_punchItemPa[_testPlant].Guid, Category.PB, RowVersion);

        _dut = new UpdatePunchItemCategoryCommandHandler(
            _punchItemRepositoryMock,
            _unitOfWorkMock,
            Substitute.For<ILogger<UpdatePunchItemCategoryCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldChangePaPunchItemToPb()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(Category.PB, _punchItemPa[_testPlant].Category);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Substitute this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result.Data);
        Assert.AreEqual(_command.RowVersion, _punchItemPa[_testPlant].RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPunchItemUpdatedCategoryDomainEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_punchItemPa[_testPlant].DomainEvents.Last(), typeof(PunchItemCategoryUpdatedDomainEvent));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPunchItemUpdatedCategoryDomainEvent_WithOneChange()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedCategoryDomainEvent = _punchItemPa[_testPlant].DomainEvents.Last() as PunchItemCategoryUpdatedDomainEvent;
        Assert.IsNotNull(punchItemUpdatedCategoryDomainEvent);
        Assert.IsNotNull(punchItemUpdatedCategoryDomainEvent.Change);
        Assert.AreEqual(nameof(PunchItem.Category), punchItemUpdatedCategoryDomainEvent.Change.Name);
        Assert.AreEqual(Category.PA.ToString(), punchItemUpdatedCategoryDomainEvent.Change.OldValue);
        Assert.AreEqual(Category.PB.ToString(), punchItemUpdatedCategoryDomainEvent.Change.NewValue);
    }
}
