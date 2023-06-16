using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.CommentAggregate;

[TestClass]
public class CommentTests
{
    private Comment _dut;
    private readonly string _sourceType = "X";
    private readonly Guid _sourceGuid = Guid.NewGuid();
    private readonly string _text = "A";

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
}
