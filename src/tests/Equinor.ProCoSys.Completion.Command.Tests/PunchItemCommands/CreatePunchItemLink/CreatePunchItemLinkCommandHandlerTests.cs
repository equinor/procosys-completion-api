using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItemLink;

[TestClass]
public class CreatePunchItemLinkCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _guid = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private CreatePunchItemLinkCommandHandler _dut;
    private CreatePunchItemLinkCommand _command;
    private Mock<ILinkService> _linkServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new CreatePunchItemLinkCommand(Guid.NewGuid(), "T", "U");

        _linkServiceMock = new Mock<ILinkService>();
        _linkServiceMock.Setup(l => l.AddAsync(
            nameof(PunchItem),
            _command.PunchItemGuid,
            _command.Title,
            _command.Url, 
            default)).ReturnsAsync(new LinkDto(_guid, _rowVersion));

        _dut = new CreatePunchItemLinkCommandHandler(_linkServiceMock.Object);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReturn_GuidAndRowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(GuidAndRowVersion));
        Assert.AreEqual(_rowVersion, result.Data.RowVersion);
        Assert.AreEqual(_guid, result.Data.Guid);
    }

    [TestMethod]
    public async Task HandlingCommand_Should_CallAdd_OnLinkService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _linkServiceMock.Verify(u => u.AddAsync(
            nameof(PunchItem), 
            _command.PunchItemGuid, 
            _command.Title,
            _command.Url,
            default), Times.Exactly(1));
    }
}
