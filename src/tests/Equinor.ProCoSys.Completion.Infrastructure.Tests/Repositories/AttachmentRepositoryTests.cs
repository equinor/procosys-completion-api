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
    private const string KnownFileName = "a.txt";
    private readonly Guid _knownParentGuid = Guid.NewGuid();

    protected override EntityWithGuidRepository<Attachment> GetDut()
        => new AttachmentRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var attachment = new Attachment("Whatever", _knownParentGuid, TestPlant, KnownFileName);
        _knownGuid = attachment.Guid;
        attachment.SetProtectedIdForTesting(_knownId);

        var attachments = new List<Attachment> { attachment };

        _dbSetMock = attachments.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Attachments
            .Returns(_dbSetMock);
    }

    protected override Attachment GetNewEntity() => new("Whatever", Guid.NewGuid(), TestPlant, "new-file.txt");

    [TestMethod]
    public async Task GetAttachmentWithFileNameForParent_KnownFileName_ShouldReturnAttachment()
    {
        // Arrange
        var dut = new AttachmentRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetAttachmentWithFileNameForParentAsync(_knownParentGuid, KnownFileName, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(KnownFileName, result.FileName);
    }

    [TestMethod]
    public async Task GetAttachmentWithFileNameForParent_UnknownFileName_ShouldReturnNull()
    {
        // Arrange
        var dut = new AttachmentRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetAttachmentWithFileNameForParentAsync(_knownParentGuid, "abc.pdf", default);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAttachmentWithFileNameForParent_UnknownParent_ShouldReturnNull()
    {
        // Arrange
        var dut = new AttachmentRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetAttachmentWithFileNameForParentAsync(Guid.NewGuid(), KnownFileName, default);

        // Assert
        Assert.IsNull(result);
    }
}
