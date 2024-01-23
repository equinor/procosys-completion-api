using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.LabelAggregate;

[TestClass]
public class LabelTests : IModificationAuditableTests
{
    private Label _dut;
    private readonly string _text = "FYI";

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Label(_text);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_text, _dut.Text);
        Assert.IsNotNull(_dut.AvailableFor);
        Assert.AreEqual(0, _dut.AvailableFor.Count);
    }

    [TestMethod]
    public void MakeLabelAvailableFor_ShouldAddLabelEntityToLabelAvailableFor()
    {
        // Arrange
        var labelEntity = new LabelEntity(EntityTypeWithLabel.PunchComment);

        // Act
        _dut.MakeLabelAvailableFor(labelEntity);

        // Assert
        Assert.AreEqual(1, _dut.AvailableFor.Count);
        Assert.IsTrue(_dut.AvailableFor.Contains(labelEntity));
    }

    [TestMethod]
    public void RemoveLabelAvailableFor_ShouldRemoveLabelEntityFromLabelAvailableFor()
    {
        // Arrange
        var labelEntity = new LabelEntity(EntityTypeWithLabel.PunchComment);
        _dut.MakeLabelAvailableFor(labelEntity);
        Assert.AreEqual(1, _dut.AvailableFor.Count);

        // Act
        _dut.RemoveLabelAvailableFor(labelEntity);

        // Assert
        Assert.AreEqual(0, _dut.AvailableFor.Count);
    }
}
