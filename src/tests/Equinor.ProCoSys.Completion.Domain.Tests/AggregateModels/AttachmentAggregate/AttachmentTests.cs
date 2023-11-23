using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.AttachmentAggregate;

[TestClass]
public class AttachmentTests : IModificationAuditableTests
{
    private Attachment _dut;
    private readonly string _parentType = "X";
    private readonly Guid _parentGuid = Guid.NewGuid();
    private readonly string _fileName = "a.txt";

    protected override ICreationAuditable GetCreationAuditable() => _dut;

    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Attachment(_parentType, _parentGuid, "PCS$Plant", _fileName);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_fileName, _dut.FileName);
        Assert.AreEqual($"Plant/X/{_dut.Guid}", _dut.BlobPath);
        Assert.AreEqual(_parentType, _dut.ParentType);
        Assert.AreEqual(_parentGuid, _dut.ParentGuid);
        Assert.AreNotEqual(_parentGuid, _dut.Guid);
        Assert.AreNotEqual(Guid.Empty, _dut.Guid);
        Assert.AreEqual(1, _dut.RevisionNumber);
    }

    [TestMethod]
    public void IncreaseRevisionNumber_ShouldIncreaseRevisionNumber()
    {
        // Act
        _dut.IncreaseRevisionNumber();

        // Arrange
        Assert.AreEqual(2, _dut.RevisionNumber);
    }
}
