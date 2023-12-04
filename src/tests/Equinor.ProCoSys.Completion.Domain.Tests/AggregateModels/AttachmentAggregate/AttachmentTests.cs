using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
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
        Assert.AreEqual(_fileName, _dut.Description);
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

    [TestMethod]
    public void UpdateLabels_ShouldAddLabelsToLabelsList()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B");
        var labelC = new Label("C");

        // Act
        _dut.UpdateLabels(new List<Label>{labelA, labelC, labelB});

        // Arrange
        Assert.AreEqual(3, _dut.Labels.Count);
        Assert.AreEqual(labelA, _dut.Labels.ElementAt(0));
        Assert.AreEqual(labelC, _dut.Labels.ElementAt(1));
        Assert.AreEqual(labelB, _dut.Labels.ElementAt(2));
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public void OrderedLabels_ShouldReturnLabelsOrdered()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B");
        var labelC = new Label("C");
        _dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });

        // Act
        var orderedLabels = _dut.OrderedLabels();

        // Arrange
        Assert.AreEqual(3, orderedLabels.Count());
        Assert.AreEqual(labelA, orderedLabels.ElementAt(0));
        Assert.AreEqual(labelB, orderedLabels.ElementAt(1));
        Assert.AreEqual(labelC, orderedLabels.ElementAt(2));
    }

    [TestMethod]
    public void UpdateLabels_ShouldRemoveLabelsFromLabelsList()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B");
        var labelC = new Label("C");
        _dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });
        Assert.AreEqual(3, _dut.Labels.Count);

        // Act
        _dut.UpdateLabels(new List<Label> { labelC });

        // Arrange
        Assert.AreEqual(1, _dut.Labels.Count);
        Assert.AreEqual(labelC, _dut.Labels.ElementAt(0));
    }

    [TestMethod]
    public void UpdateLabels_ShouldAddNewLabelsAtEnd()
    {
        // Arrange
        var labelA = new Label("A");
        var labelB = new Label("B");
        var labelC = new Label("C");
        var labelD = new Label("D");
        _dut.UpdateLabels(new List<Label> { labelA, labelC, labelB });
        Assert.AreEqual(3, _dut.Labels.Count);

        // Act
        _dut.UpdateLabels(new List<Label> { labelD, labelB });

        // Arrange
        Assert.AreEqual(2, _dut.Labels.Count);
        Assert.AreEqual(labelB, _dut.Labels.ElementAt(0));
        Assert.AreEqual(labelD, _dut.Labels.ElementAt(1));
    }
}
