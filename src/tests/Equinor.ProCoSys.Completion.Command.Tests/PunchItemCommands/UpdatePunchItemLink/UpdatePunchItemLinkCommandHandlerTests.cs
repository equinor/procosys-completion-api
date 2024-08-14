using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItemLink;

[TestClass]
public class UpdatePunchItemLinkCommandHandlerTests : PunchItemCommandTestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private UpdatePunchItemLinkCommandHandler _dut;
    private UpdatePunchItemLinkCommand _command;
    private ILinkService _linkServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new UpdatePunchItemLinkCommand(Guid.NewGuid(), Guid.NewGuid(), "T", "U", _rowVersion)
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };


        _linkServiceMock = Substitute.For<ILinkService>();
        _linkServiceMock.UpdateAsync(
            _command.LinkGuid,
            _command.Title,
            _command.Url,
            _command.RowVersion,
            default).Returns(_rowVersion);

        _dut = new UpdatePunchItemLinkCommandHandler(_linkServiceMock);
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
        await _linkServiceMock.Received(1).UpdateAsync(
            _command.LinkGuid,
            _command.Title,
            _command.Url,
            _command.RowVersion,
            default);
    }
}
