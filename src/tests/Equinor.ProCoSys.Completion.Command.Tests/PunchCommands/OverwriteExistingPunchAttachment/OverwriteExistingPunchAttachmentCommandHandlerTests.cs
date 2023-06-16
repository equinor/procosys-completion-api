using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.OverwriteExistingPunchAttachment;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.OverwriteExistingPunchAttachment;

[TestClass]
public class OverwriteExistingPunchAttachmentCommandHandlerTests : TestsBase
{
    private readonly string _newRowVersion = "AAAAAAAAACC=";
    private OverwriteExistingPunchAttachmentCommandHandler _dut;
    private OverwriteExistingPunchAttachmentCommand _command;
    private Mock<IAttachmentService> _attachmentServiceMock;

    [TestInitialize]
    public void Setup()
    {
        var oldRowVersion = "AAAAAAAAABA=";
        _command = new OverwriteExistingPunchAttachmentCommand(Guid.NewGuid(), "T", oldRowVersion, new MemoryStream());

        _attachmentServiceMock = new Mock<IAttachmentService>();
        _attachmentServiceMock.Setup(a => a.UploadOverwriteAsync(
            nameof(Punch),
            _command.PunchGuid,
            _command.FileName,
            _command.Content,
            _command.RowVersion,
            default)).ReturnsAsync(_newRowVersion);

        _dut = new OverwriteExistingPunchAttachmentCommandHandler(_attachmentServiceMock.Object);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReturn_NewGuid()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_newRowVersion, result.Data);
    }

    [TestMethod]
    public async Task HandlingCommand_Should_CallAdd_OnAttachmentService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _attachmentServiceMock.Verify(u => u.UploadOverwriteAsync(
            nameof(Punch), 
            _command.PunchGuid, 
            _command.FileName,
            _command.Content,
            _command.RowVersion,
            default), Times.Exactly(1));
    }
}
