using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.CommentAggregate;

[TestClass]
public class CommentTests : ICreationAuditableTests
{
    private Comment _dut;
    private readonly string _parentType = "X";
    private readonly Guid _parentGuid = Guid.NewGuid();
    private readonly string _text = "A";

    protected override ICreationAuditable GetCreationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Comment(_parentType, _parentGuid, _text);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_text, _dut.Text);
        Assert.AreEqual(_parentType, _dut.ParentType);
        Assert.AreEqual(_parentGuid, _dut.ParentGuid);
        Assert.AreNotEqual(_parentGuid, _dut.Guid);
        Assert.AreNotEqual(Guid.Empty, _dut.Guid);
    }
}
