using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunchLink;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.UpdatePunchLink;

[TestClass]
public class UpdatePunchLinkCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private UpdatePunchLinkCommandHandler _dut;
    private UpdatePunchLinkCommand _command;
    private Mock<ILinkService> _linkServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new UpdatePunchLinkCommand(Guid.NewGuid(), Guid.NewGuid(), "T", "U", _rowVersion);

        _linkServiceMock = new Mock<ILinkService>();
        _linkServiceMock.Setup(l => l.UpdateAsync(
            _command.LinkGuid,
            _command.Title,
            _command.Url,
            _command.RowVersion,
            default)).ReturnsAsync(_rowVersion);

        _dut = new UpdatePunchLinkCommandHandler(_linkServiceMock.Object);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReturn_RowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(string));
        Assert.AreEqual(_rowVersion, result.Data);
    }

    [TestMethod]
    public async Task HandlingCommand_Should_CallUpdate_OnLinkService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _linkServiceMock.Verify(u => u.UpdateAsync(
            _command.LinkGuid,
            _command.Title,
            _command.Url,
            _command.RowVersion,
            default), Times.Exactly(1));
    }
}
