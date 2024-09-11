using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UploadNewPunchItemAttachment;

[TestClass]
public class UploadNewPunchItemAttachmentCommandHandlerTests : PunchItemCommandTestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _guid = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private UploadNewPunchItemAttachmentCommandHandler _dut;
    private UploadNewPunchItemAttachmentCommand _command;
    private IAttachmentService _attachmentServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new UploadNewPunchItemAttachmentCommand(Guid.NewGuid(), "T", new MemoryStream(), "image/jpeg")
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _attachmentServiceMock.UploadNewAsync(
            _command.PunchItem.Project.Name,
            nameof(PunchItem),
            _command.PunchItemGuid,
            _command.FileName,
            _command.Content,
            _command.ContentType,
            default).Returns(new AttachmentDto(_guid, _rowVersion));

        _dut = new UploadNewPunchItemAttachmentCommandHandler(_attachmentServiceMock);
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
    public async Task HandlingCommand_Should_CallUploadNewAsync_OnAttachmentService()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _attachmentServiceMock.Received(1).UploadNewAsync(
            _command.PunchItem.Project.Name,
            nameof(PunchItem), 
            _command.PunchItemGuid, 
            _command.FileName,
            _command.Content,
            _command.ContentType,
            default);
    }
}
