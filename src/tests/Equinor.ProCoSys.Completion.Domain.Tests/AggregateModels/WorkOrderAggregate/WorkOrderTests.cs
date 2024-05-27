using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.WorkOrderAggregate;

[TestClass]
public class WorkOrderTests 
{
    private WorkOrder _dut;
    private readonly string _testPlant = "PlantA";
    private readonly string _no = "0001";
    private readonly Guid _guid = Guid.NewGuid();
    
    [TestInitialize]
    public void Setup() => _dut = new WorkOrder(_testPlant, _guid, _no);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_testPlant, _dut.Plant);
        Assert.AreEqual(_no, _dut.No);
        Assert.AreEqual(_guid, _dut.Guid);
    }
}
