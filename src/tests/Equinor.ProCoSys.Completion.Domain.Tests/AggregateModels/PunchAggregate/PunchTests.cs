using System;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.PunchAggregate;

[TestClass]
public class PunchTests
{
    private Punch _dut;
    private readonly string _testPlant = "PlantA";
    private readonly int _projectId = 132;
    private Project _project;
    private readonly string _title = "Title A";

    [TestInitialize]
    public void Setup()
    {
        _project = new(_testPlant, Guid.NewGuid(), "P", "D");
        _project.SetProtectedIdForTesting(_projectId);
        TimeService.SetProvider(
            new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));

        _dut = new Punch(_testPlant, _project, _title); 
    }

    #region Constructor
    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_testPlant, _dut.Plant);
        Assert.AreEqual(_projectId, _dut.ProjectId);
        Assert.AreEqual(_title, _dut.Title);
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenProjectNotGiven() =>
        Assert.ThrowsException<ArgumentNullException>(() =>
            new Punch(_testPlant, null!, _title));

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenProjectInOtherPlant()
        => Assert.ThrowsException<ArgumentException>(() =>
            new Punch(_testPlant, new Project("OtherPlant", Guid.NewGuid(), "P", "D"), _title));
    #endregion

    #region Update

    [TestMethod]
    public void Update_ShouldUpdate()
    {
        _dut.Update("New Title", "New Text");

        Assert.AreEqual("New Title", _dut.Title);
        Assert.AreEqual("New Text", _dut.Text);
    }
    #endregion
}
