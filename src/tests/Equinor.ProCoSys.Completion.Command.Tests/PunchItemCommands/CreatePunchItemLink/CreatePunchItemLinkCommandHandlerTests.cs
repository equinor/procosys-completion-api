using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItemLink;

[TestClass]
public class CreatePunchItemLinkCommandHandlerTests : PunchItemCommandTestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _guid = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private CreatePunchItemLinkCommandHandler _dut;
    private CreatePunchItemLinkCommand _command;
    private ILinkService _linkServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new CreatePunchItemLinkCommand(Guid.NewGuid(), "T", "U")
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _linkServiceMock = Substitute.For<ILinkService>();
        _linkServiceMock.AddAsync(
            nameof(PunchItem),
            _command.PunchItemGuid,
            _command.Title,
            _command.Url, 
            default).Returns(new LinkDto(_guid, _rowVersion));

        _dut = new CreatePunchItemLinkCommandHandler(_linkServiceMock);
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
        await _linkServiceMock.Received(1)
            .AddAsync(
            nameof(PunchItem), 
            _command.PunchItemGuid, 
            _command.Title,
            _command.Url,
            default);
    }
}
