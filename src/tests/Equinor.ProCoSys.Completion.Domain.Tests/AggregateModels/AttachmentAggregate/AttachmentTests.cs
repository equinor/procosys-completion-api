using System;
using System.Threading;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.AttachmentAggregate;

[TestClass]
public class AttachmentTests : IModificationAuditableTests
{
    private Attachment _dut;
    private readonly string _sourceType = "X";
    private readonly Guid _sourceGuid = Guid.NewGuid();
    private readonly string _fileName = "a.txt";

    protected override ICreationAuditable GetCreationAuditable() => _dut;

    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Attachment(_sourceType, _sourceGuid, "PCS$Plant", _fileName);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_fileName, _dut.FileName);
        Assert.AreEqual($"Plant/X/{_dut.Guid}", _dut.BlobPath);
        Assert.AreEqual(_sourceType, _dut.SourceType);
        Assert.AreEqual(_sourceGuid, _dut.SourceGuid);
        Assert.AreNotEqual(_sourceGuid, _dut.Guid);
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

    [TestMethod]
    public void Constructor_ShouldCreateSequentialUniqueIdentifiers()
    {
        // Arrange
        var prevGuid = new Attachment(_sourceType, _sourceGuid, "PCS$Plant", _fileName).Guid;

        // Act and Assert
        for (var i = 0; i < 20; i++)
        {
            var nextGuid = new Attachment(_sourceType, _sourceGuid, "PCS$Plant", _fileName).Guid;
            Assert.IsTrue(prevGuid < nextGuid);
            Console.WriteLine(prevGuid);
            prevGuid = nextGuid;
            Thread.Sleep(5);
        }
    }
}
