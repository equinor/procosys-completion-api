using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchLink;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.DeletePunchLink;

[TestClass]
public class DeletePunchLinkCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private DeletePunchLinkCommandHandler _dut;
    private DeletePunchLinkCommand _command;
    private Mock<ILinkService> _linkServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new DeletePunchLinkCommand(Guid.NewGuid(), Guid.NewGuid(), _rowVersion);

        _linkServiceMock = new Mock<ILinkService>();


        _dut = new DeletePunchLinkCommandHandler(_linkServiceMock.Object);
    }

    [TestMethod]
    public async Task HandlingCommand_Should_CallDelete_OnLinkService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _linkServiceMock.Verify(u => u.DeleteAsync(
            _command.LinkGuid,
            _command.RowVersion,
            default), Times.Exactly(1));
    }
}
