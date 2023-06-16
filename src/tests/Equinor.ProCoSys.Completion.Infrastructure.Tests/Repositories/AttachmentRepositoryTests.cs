using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class AttachmentRepositoryTests : EntityWithGuidRepositoryTestBase<Attachment>
{
    private new AttachmentRepository _dut;
    private readonly string _knownFileName = "a.txt";
    private readonly Guid _knownSourceGuid = Guid.NewGuid();

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var attachment = new Attachment("Whatever", _knownSourceGuid, TestPlant, _knownFileName);
        _knownGuid = attachment.Guid;
        attachment.SetProtectedIdForTesting(_knownId);

        var attachments = new List<Attachment> { attachment };

        _dbSetMock = attachments.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Setup(x => x.Attachments)
            .Returns(_dbSetMock.Object);

        _dut = new AttachmentRepository(_contextHelper.ContextMock.Object);
        base._dut = _dut;
    }

    protected override Attachment GetNewEntity() => new("Whatever", Guid.NewGuid(), TestPlant, "new-file.txt");

    [TestMethod]
    public async Task TryGetAttachmentWithFilenameForSource_KnownFileName_ShouldReturnAttachment()
    {
        var result = await _dut.TryGetAttachmentWithFilenameForSourceAsync(_knownSourceGuid, _knownFileName);

        Assert.IsNotNull(result);
        Assert.AreEqual(_knownFileName, result.FileName);
    }

    [TestMethod]
    public async Task TryGetAttachmentWithFilenameForSource_UnknownFileName_ShouldReturnNull()
    {
        var result = await _dut.TryGetAttachmentWithFilenameForSourceAsync(_knownSourceGuid, "abc.pdf");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task TryGetAttachmentWithFilenameForSource_UnknownSource_ShouldReturnNull()
    {
        var result = await _dut.TryGetAttachmentWithFilenameForSourceAsync(Guid.NewGuid(), _knownFileName);

        Assert.IsNull(result);
    }
}
