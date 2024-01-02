using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
 using NSubstitute;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItemComment;

[TestClass]
public class CreatePunchItemCommentCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _guid = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private CreatePunchItemCommentCommandHandler _dut;
    private CreatePunchItemCommentCommand _command;
    private ICommentService _commentServiceMock;
    private ILabelRepository _labelRepositoryMock;
    private List<Label> _labelList;

    [TestInitialize]
    public void Setup()
    {
        var labelText = "a";
        _command = new CreatePunchItemCommentCommand(Guid.NewGuid(), "T", new List<string> { labelText });

        _labelRepositoryMock = Substitute.For<ILabelRepository>();
        _labelList = new List<Label> { new(labelText) };
        _labelRepositoryMock.GetManyAsync(_command.Labels, default).Returns(_labelList);

        _commentServiceMock = Substitute.For<ICommentService>();
        _commentServiceMock.AddAsync(
            nameof(PunchItem),
            _command.PunchItemGuid,
            _command.Text,
            _labelList,
            default).Returns(new CommentDto(_guid, _rowVersion));

        _dut = new CreatePunchItemCommentCommandHandler(_commentServiceMock, _labelRepositoryMock);
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
    public async Task HandlingCommand_Should_CallAdd_OnCommentService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _commentServiceMock.Received(1)
            .AddAsync(
            nameof(PunchItem), 
            _command.PunchItemGuid, 
            _command.Text,
            _labelList,
            default);
    }
}
