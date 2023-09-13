using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItem;

[TestClass]
public class UpdatePunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private UpdatePunchItemCommand _command;
    private UpdatePunchItemCommandHandler _dut;
    private readonly string _newDescription = "new description";

    [TestInitialize]
    public void Setup()
    {
        var jsonPatchDocument = new JsonPatchDocument();
        //jsonPatchDocument.Replace("/Tord", _newDescription);
        jsonPatchDocument.Replace($"/{nameof(PunchItem.Description)}", _newDescription);
        //jsonPatchDocument.Replace($"/{nameof(PunchItem.Id)}", 1);
        _command = new UpdatePunchItemCommand(_existingPunchItem.Guid, jsonPatchDocument);

        _dut = new UpdatePunchItemCommandHandler(
            _punchItemRepositoryMock,
            _unitOfWorkMock,
            Substitute.For<ILogger<UpdatePunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdatePunchItem()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_newDescription, _existingPunchItem.Description);
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
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(RowVersion, result.Data);
        Assert.AreEqual(RowVersion, _existingPunchItem.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPunchItemUpdatedEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_existingPunchItem.DomainEvents.Last(), typeof(PunchItemUpdatedDomainEvent));
    }
}
