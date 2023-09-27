using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
 using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.OverwriteExistingPunchItemAttachment;

[TestClass]
public class OverwriteExistingPunchItemAttachmentCommandHandlerTests : TestsBase
{
    private readonly string _newRowVersion = "AAAAAAAAACC=";
    private OverwriteExistingPunchItemAttachmentCommandHandler _dut;
    private OverwriteExistingPunchItemAttachmentCommand _command;
    private IAttachmentService _attachmentServiceMock;

    [TestInitialize]
    public void Setup()
    {
        var oldRowVersion = "AAAAAAAAABA=";
        _command = new OverwriteExistingPunchItemAttachmentCommand(Guid.NewGuid(), "T", oldRowVersion, new MemoryStream());

        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _attachmentServiceMock.UploadOverwriteAsync(
            nameof(PunchItem),
            _command.PunchItemGuid,
            _command.FileName,
            _command.Content,
            _command.RowVersion,
            default).Returns(_newRowVersion);

        _dut = new OverwriteExistingPunchItemAttachmentCommandHandler(_attachmentServiceMock);
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
        await _attachmentServiceMock.Received(1).UploadOverwriteAsync(
            nameof(PunchItem), 
            _command.PunchItemGuid, 
            _command.FileName,
            _command.Content,
            _command.RowVersion,
            default);
    }
}
