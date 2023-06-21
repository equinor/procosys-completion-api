using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.PunchAggregate;

[TestClass]
public class PunchTests : IModificationAuditableTests
{
    private Punch _dut;
    private readonly string _testPlant = "PlantA";
    private readonly int _projectId = 132;
    private Project _project;
    private readonly string _itemNo = "Item A";

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup()
    {
        _project = new Project(_testPlant, Guid.NewGuid(), "P", "D");
        _project.SetProtectedIdForTesting(_projectId);
        _dut = new Punch(_testPlant, _project, _itemNo); 
    }

    #region Constructor
    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_testPlant, _dut.Plant);
        Assert.AreEqual(_projectId, _dut.ProjectId);
        Assert.AreEqual(_itemNo, _dut.ItemNo);
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenProjectNotGiven() =>
        Assert.ThrowsException<ArgumentNullException>(() =>
            new Punch(_testPlant, null!, _itemNo));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenProjectInOtherPlant()
        => Assert.ThrowsException<ArgumentException>(() =>
            new Punch(_testPlant, new Project("OtherPlant", Guid.NewGuid(), "P", "D"), _itemNo));
    #endregion

    #region Update

    [TestMethod]
    public void Update_ShouldUpdate()
    {
        _dut.Update("ABC");

        Assert.AreEqual("ABC", _dut.Description);
    }
    #endregion
}
