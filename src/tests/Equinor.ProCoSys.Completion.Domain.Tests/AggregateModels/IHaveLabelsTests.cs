using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels;

[TestClass]
public abstract class IHaveLabelsTests
{
    protected abstract IHaveLabels GetEntityWithLabels();

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        // Act
        var dut = GetEntityWithLabels();

        // Assert
        Assert.AreEqual(0, dut.Labels.Count);
        Assert.AreEqual(0, dut.GetOrderedNonVoidedLabels().Count());
    }

    [TestMethod]
    public void UpdateLabels_ShouldAddLabelsToLabelsList()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B");
        var labelC = new Label("C");
        var dut = GetEntityWithLabels();

        // Act
        dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });

        // Arrange
        Assert.AreEqual(3, dut.Labels.Count);
        Assert.IsTrue(dut.Labels.Any(l => l == labelA));
        Assert.IsTrue(dut.Labels.Any(l => l == labelB));
        Assert.IsTrue(dut.Labels.Any(l => l == labelC));
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public void GetOrderedNonVoidedLabels_ShouldReturnLabelsOrdered()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B");
        var labelC = new Label("C");
        var dut = GetEntityWithLabels();
        dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });

        // Act
        var orderedNonVoidedLabels = dut.GetOrderedNonVoidedLabels();

        // Arrange
        Assert.AreEqual(3, orderedNonVoidedLabels.Count());
        Assert.AreEqual(labelA, orderedNonVoidedLabels.ElementAt(0));
        Assert.AreEqual(labelB, orderedNonVoidedLabels.ElementAt(1));
        Assert.AreEqual(labelC, orderedNonVoidedLabels.ElementAt(2));
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public void GetOrderedNonVoidedLabels_ShouldNotReturnVoidedLabels()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B") { IsVoided = true };
        var labelC = new Label("C");
        var dut = GetEntityWithLabels();
        dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });

        // Act
        var orderedNonVoidedLabels = dut.GetOrderedNonVoidedLabels();

        // Arrange
        Assert.AreEqual(2, orderedNonVoidedLabels.Count());
        Assert.AreEqual(labelA, orderedNonVoidedLabels.ElementAt(0));
        Assert.AreEqual(labelC, orderedNonVoidedLabels.ElementAt(1));
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public void Labels_ShouldReturnVoidedLabels()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B") { IsVoided = true };
        var labelC = new Label("C");
        var dut = GetEntityWithLabels();
        dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });

        // Act
        var labels = dut.Labels;

        // Arrange
        Assert.AreEqual(3, labels.Count);
        Assert.IsTrue(labels.Any(l => l == labelA));
        Assert.IsTrue(labels.Any(l => l == labelB));
        Assert.IsTrue(labels.Any(l => l == labelC));
    }

    [TestMethod]
    public void UpdateLabels_ShouldRemoveLabelsFromLabelsList()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B");
        var labelC = new Label("C");
        var dut = GetEntityWithLabels();
        dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });
        Assert.AreEqual(3, dut.Labels.Count);

        // Act
        dut.UpdateLabels(new List<Label> { labelC });

        // Arrange
        Assert.AreEqual(1, dut.Labels.Count);
        Assert.AreEqual(labelC, dut.Labels.ElementAt(0));
    }

    [TestMethod]
    public void UpdateLabels_ShouldBothRemoveAndAddLabels()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B");
        var labelC = new Label("C");
        var labelD = new Label("D");
        var dut = GetEntityWithLabels();
        dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });
        Assert.AreEqual(3, dut.Labels.Count);

        // Act
        dut.UpdateLabels(new List<Label> { labelD, labelB });

        // Arrange
        Assert.AreEqual(2, dut.Labels.Count);
        Assert.IsTrue(dut.Labels.Any(l => l == labelB));
        Assert.IsTrue(dut.Labels.Any(l => l == labelD));
    }
}
