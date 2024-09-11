using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItemComment;

[TestClass]
public class CreatePunchItemCommentCommandHandlerTests : PunchItemCommandTestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _guid = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private CreatePunchItemCommentCommandHandler _dut;
    private CreatePunchItemCommentCommand _command;
    private ICommentService _commentServiceMock;
    private List<Label> _labelList;
    private List<Person> _personList;

    [TestInitialize]
    public void Setup()
    {
        var labelText = "a";
        var person = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);
        _personList = [person];
        _command = new CreatePunchItemCommentCommand(
            Guid.NewGuid(),
            "T",
            new List<string> { labelText },
            new List<Guid> { person.Guid })
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        var labelRepositoryMock = Substitute.For<ILabelRepository>();
        _labelList = [new(labelText)];
        labelRepositoryMock.GetManyAsync(_command.Labels, default).Returns(_labelList);
        var personRepositoryMock = Substitute.For<IPersonRepository>();
        personRepositoryMock.GetOrCreateManyAsync(_command.Mentions, default)
            .Returns(_personList);

        _commentServiceMock = Substitute.For<ICommentService>();
        _commentServiceMock.AddAsync(
            _unitOfWorkMock,
            _command.PunchItem,
            Arg.Any<string>(),
            _command.Text,
            _labelList,
            _personList,
            MailTemplateCode.PunchCommented,
            default).Returns(new CommentDto(_guid, _rowVersion));

        _dut = new CreatePunchItemCommentCommandHandler(
            _commentServiceMock, 
            labelRepositoryMock, 
            personRepositoryMock, 
            _unitOfWorkMock);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReturn_GuidAndRowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(result, typeof(GuidAndRowVersion));
        Assert.AreEqual(_rowVersion, result.RowVersion);
        Assert.AreEqual(_guid, result.Guid);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddAndSaveComment()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _commentServiceMock.Received(1)
            .AddAsync(
                _unitOfWorkMock,
                _command.PunchItem,
                Arg.Any<string>(),
                _command.Text,
                _labelList,
                _personList,
                MailTemplateCode.PunchCommented,
                default);
    }
}
