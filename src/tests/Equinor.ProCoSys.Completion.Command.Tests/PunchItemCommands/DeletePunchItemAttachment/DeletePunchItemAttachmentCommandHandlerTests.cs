using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DeletePunchItemAttachment;

[TestClass]
public class DeletePunchItemAttachmentCommandHandlerTests
{
    private DeletePunchItemAttachmentCommandHandler _dut;
    private DeletePunchItemAttachmentCommand _command;
    private IAttachmentService _attachmentServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new DeletePunchItemAttachmentCommand(Guid.NewGuid(), Guid.NewGuid(), "r");

        _attachmentServiceMock = Substitute.For<IAttachmentService>();

        _dut = new DeletePunchItemAttachmentCommandHandler(_attachmentServiceMock);
    }

    [TestMethod]
    public async Task HandlingCommand_Should_CallDeleteAsync_OnAttachmentService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _attachmentServiceMock.Received(1).DeleteAsync(
            _command.AttachmentGuid,
            _command.RowVersion,
            default);
    }
}
