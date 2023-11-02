using System;
using System.Threading;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.CommentAggregate;

[TestClass]
public class CommentTests : ICreationAuditableTests
{
    private Comment _dut;
    private readonly string _sourceType = "X";
    private readonly Guid _sourceGuid = Guid.NewGuid();
    private readonly string _text = "A";

    protected override ICreationAuditable GetCreationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Comment(_sourceType, _sourceGuid, _text);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_text, _dut.Text);
        Assert.AreEqual(_sourceType, _dut.SourceType);
        Assert.AreEqual(_sourceGuid, _dut.SourceGuid);
        Assert.AreNotEqual(_sourceGuid, _dut.Guid);
        Assert.AreNotEqual(Guid.Empty, _dut.Guid);
    }

    [TestMethod]
    public void Constructor_ShouldCreateSequentialUniqueIdentifiers()
    {
        // Arrange
        var prevGuid = new Comment(_sourceType, _sourceGuid, _text).Guid;

        // Act and Assert
        for (var i = 0; i < 20; i++)
        {
            var nextGuid = new Comment(_sourceType, _sourceGuid, _text).Guid;
            Assert.IsTrue(prevGuid < nextGuid);
            Console.WriteLine(prevGuid);
            prevGuid = nextGuid;
            Thread.Sleep(5);
        }
    }
}
