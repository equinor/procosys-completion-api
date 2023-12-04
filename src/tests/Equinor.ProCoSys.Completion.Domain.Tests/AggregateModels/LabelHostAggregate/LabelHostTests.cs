using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelHostAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.LabelHostAggregate;

[TestClass]
public class LabelHostTests : IModificationAuditableTests
{
    private LabelHost _dut;
    private readonly HostType _type = HostType.PunchComment;

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new LabelHost(_type);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_type, _dut.Type);
        Assert.IsNotNull(_dut.Labels);
        Assert.AreEqual(0, _dut.Labels.Count);
    }

    [TestMethod]
    public void AddLabel_ShouldAddLabelToLabelsList()
    {
        var label = new Label("FYI");

        _dut.AddLabel(label);

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
