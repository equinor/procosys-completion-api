using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItemAttachment;

[TestClass]
public class UpdatePunchItemAttachmentCommandHandlerTests : PunchItemCommandTestsBase
{
    private UpdatePunchItemAttachmentCommandHandler _dut;
    private UpdatePunchItemAttachmentCommand _command;
    private IAttachmentService _attachmentServiceMock;
    private ILabelRepository _labelRepositoryMock;
    private List<Label> _labelList;

    [TestInitialize]
    public void Setup()
    {
        var labelText = "a";
        _command = new UpdatePunchItemAttachmentCommand(Guid.NewGuid(), Guid.NewGuid(), "d", new List<string> { labelText }, "r")
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _labelRepositoryMock = Substitute.For<ILabelRepository>();
        _labelList = [new(labelText)];
        _labelRepositoryMock.GetManyAsync(_command.Labels, default).Returns(_labelList);

        _dut = new UpdatePunchItemAttachmentCommandHandler(_attachmentServiceMock, _labelRepositoryMock);
    }

    [TestMethod]
    public async Task HandlingCommand_Should_CallUpdateAsync_OnAttachmentService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _attachmentServiceMock.Received(1).UpdateAsync(
            _command.AttachmentGuid,
            _command.Description,
            _labelList,
            _command.RowVersion,
            default);
    }
}
