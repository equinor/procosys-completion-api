using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UploadNewPunchAttachment;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.UploadNewPunchAttachment;

[TestClass]
public class UploadNewPunchAttachmentCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _guid = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private UploadNewPunchAttachmentCommandHandler _dut;
    private UploadNewPunchAttachmentCommand _command;
    private Mock<IAttachmentService> _attachmentServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new UploadNewPunchAttachmentCommand(Guid.NewGuid(), "T", new MemoryStream());

        _attachmentServiceMock = new Mock<IAttachmentService>();
        _attachmentServiceMock.Setup(a => a.UploadNewAsync(
            nameof(Punch),
            _command.PunchGuid,
            _command.FileName,
            _command.Content,
            default)).ReturnsAsync(new AttachmentDto(_guid, _rowVersion));

        _dut = new UploadNewPunchAttachmentCommandHandler(_attachmentServiceMock.Object);
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
    public async Task HandlingCommand_Should_CallUploadNewAsync_OnAttachmentService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _attachmentServiceMock.Verify(u => u.UploadNewAsync(
            nameof(Punch), 
            _command.PunchGuid, 
            _command.FileName,
            _command.Content,
            default), Times.Exactly(1));
    }
}
