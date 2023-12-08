using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.LabelEntityAggregate;

[TestClass]
public class LabelEntityTests : IModificationAuditableTests
{
    private LabelEntity _dut;
    private readonly EntityWithLabelType _type = EntityWithLabelType.PunchComment;

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new LabelEntity(_type);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_type, _dut.EntityWithLabel);
        Assert.IsNotNull(_dut.Labels);
        Assert.AreEqual(0, _dut.Labels.Count);
    }

    [TestMethod]
    public void AddLabel_ShouldAddLabelToLabelsList()
    {
        // Arrange
        var label = new Label("FYI");

        // Act
        _dut.AddLabel(label);

        // Assert
        Assert.AreEqual(1, _dut.Labels.Count);
        Assert.IsTrue(_dut.Labels.Contains(label));
    }

    [TestMethod]
    public void RemoveLabel_ShouldRemoveLabelFromLabelsList()
    {
        // Arrange
        var label = new Label("FYI");
        _dut.AddLabel(label);
        Assert.AreEqual(1, _dut.Labels.Count);

        // Act
        _dut.RemoveLabel(label);

        // Assert
        Assert.AreEqual(0, _dut.Labels.Count);
    }
}
