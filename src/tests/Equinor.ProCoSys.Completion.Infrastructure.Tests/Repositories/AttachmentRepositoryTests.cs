using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class AttachmentRepositoryTests : EntityWithGuidRepositoryTestBase<Attachment>
{
    private new AttachmentRepository _dut;
    private const string KnownFileName = "a.txt";
    private readonly Guid _knownSourceGuid = Guid.NewGuid();

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var attachment = new Attachment("Whatever", _knownSourceGuid, TestPlant, KnownFileName);
        _knownGuid = attachment.Guid;
        attachment.SetProtectedIdForTesting(_knownId);

        var attachments = new List<Attachment> { attachment };

        _dbSetMock = attachments.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Attachments
            .Returns(_dbSetMock);

        _dut = new AttachmentRepository(_contextHelper.ContextMock);
        base._dut = _dut;
    }

    protected override Attachment GetNewEntity() => new("Whatever", Guid.NewGuid(), TestPlant, "new-file.txt");

    [TestMethod]
    public async Task GetAttachmentWithFileNameForSource_KnownFileName_ShouldReturnAttachment()
    {
        var result = await _dut.GetAttachmentWithFileNameForSourceAsync(_knownSourceGuid, KnownFileName, default);

        Assert.IsNotNull(result);
        Assert.AreEqual(KnownFileName, result.FileName);
    }

    [TestMethod]
    public async Task GetAttachmentWithFileNameForSource_UnknownFileName_ShouldReturnNull()
    {
        var result = await _dut.GetAttachmentWithFileNameForSourceAsync(_knownSourceGuid, "abc.pdf", default);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAttachmentWithFileNameForSource_UnknownSource_ShouldReturnNull()
    {
        var result = await _dut.GetAttachmentWithFileNameForSourceAsync(Guid.NewGuid(), KnownFileName, default);

        Assert.IsNull(result);
    }
}
