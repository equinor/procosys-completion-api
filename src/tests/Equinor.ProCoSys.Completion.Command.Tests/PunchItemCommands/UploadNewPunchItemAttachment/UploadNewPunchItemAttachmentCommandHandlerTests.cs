using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
 using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UploadNewPunchItemAttachment;

[TestClass]
public class UploadNewPunchItemAttachmentCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private readonly Guid _guid = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private UploadNewPunchItemAttachmentCommandHandler _dut;
    private UploadNewPunchItemAttachmentCommand _command;
    private IAttachmentService _attachmentServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _command = new UploadNewPunchItemAttachmentCommand(Guid.NewGuid(), "T", new MemoryStream());

        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _attachmentServiceMock.UploadNewAsync(
            nameof(PunchItem),
            _command.PunchItemGuid,
            _command.FileName,
            _command.Content,
            default).Returns(new AttachmentDto(_guid, _rowVersion));

        _dut = new UploadNewPunchItemAttachmentCommandHandler(_attachmentServiceMock);
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
        await _attachmentServiceMock.Received(1).UploadNewAsync(
            nameof(PunchItem), 
            _command.PunchItemGuid, 
            _command.FileName,
            _command.Content,
            default);
    }
}
