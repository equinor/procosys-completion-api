using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DeletePunchItemLink;

[TestClass]
public class DeletePunchItemLinkCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private DeletePunchItemLinkCommandHandler _dut;
    private DeletePunchItemLinkCommand _command;
    private Mock<ILinkService> _linkServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new DeletePunchItemLinkCommand(Guid.NewGuid(), Guid.NewGuid(), _rowVersion);

        _linkServiceMock = new Mock<ILinkService>();


        _dut = new DeletePunchItemLinkCommandHandler(_linkServiceMock.Object);
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
