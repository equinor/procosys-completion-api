using System;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.LibraryAggregate;

[TestClass]
public class LibraryItemTests 
{
    private LibraryItem _dut;
    private readonly string _testPlant = "PlantA";
    private readonly string _code = "X";
    private readonly Guid _guid = Guid.NewGuid();
    private readonly string _description = "X Desc";
    private readonly LibraryType _type = LibraryType.COMPLETION_ORGANIZATION;
    private readonly Classification _classification = new(Guid.NewGuid(), "CL");

    [TestInitialize]
    public void Setup() => _dut = new LibraryItem(_testPlant, _guid, _code, _description, _type);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_code, _dut.Code);
        Assert.AreEqual(_description, _dut.Description);
        Assert.AreEqual(_type, _dut.Type);
        Assert.AreEqual(_guid, _dut.Guid);
    }

    [TestMethod]
    public void AddClassification_ShouldAddClassification()
    {
        // Act
        _dut.AddClassification(_classification);

        // Arrange
        Assert.AreEqual(1, _dut.Classifications.Count);
        Assert.AreEqual(_classification, _dut.Classifications.ElementAt(0));
    }

    [TestMethod]
    public void AddMultipleEqualClassifications_ShouldAddOnlyOneClassification()
    {
        // Act
        _dut.AddClassification(_classification);
        _dut.AddClassification(_classification);
        _dut.AddClassification(new Classification(_classification.Guid, "blah"));
        _dut.AddClassification(new Classification(Guid.NewGuid(), _classification.Name));

        // Arrange
        Assert.AreEqual(1, _dut.Classifications.Count);
        Assert.AreEqual(_classification, _dut.Classifications.ElementAt(0));
    }

    [TestMethod]
    public void RemoveClassification_ShouldRemoveClassification()
    {
        // Arrange
        _dut.AddClassification(_classification);
        Assert.AreEqual(1, _dut.Classifications.Count);

        // Act
        _dut.RemoveClassification(_classification);

        // Arrange
        Assert.AreEqual(0, _dut.Classifications.Count);
    }
}
