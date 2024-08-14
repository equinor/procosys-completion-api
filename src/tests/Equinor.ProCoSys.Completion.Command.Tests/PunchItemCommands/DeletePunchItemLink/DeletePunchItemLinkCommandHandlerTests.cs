using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DeletePunchItemLink;

[TestClass]
public class DeletePunchItemLinkCommandHandlerTests : PunchItemCommandTestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private DeletePunchItemLinkCommandHandler _dut;
    private DeletePunchItemLinkCommand _command;
    private ILinkService _linkServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new DeletePunchItemLinkCommand(Guid.NewGuid(), Guid.NewGuid(), _rowVersion)
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _linkServiceMock = Substitute.For<ILinkService>();

        _dut = new DeletePunchItemLinkCommandHandler(_linkServiceMock);
    }

    [TestMethod]
    public async Task HandlingCommand_Should_CallDelete_OnLinkService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _linkServiceMock.Received(1).DeleteAsync(
            _command.LinkGuid,
            _command.RowVersion,
            default);
    }
}
