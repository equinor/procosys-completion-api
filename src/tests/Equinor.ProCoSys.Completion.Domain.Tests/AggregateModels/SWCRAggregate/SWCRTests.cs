using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.SWCRAggregate;

[TestClass]
public class SWCRTests : IModificationAuditableTests
{
    private SWCR _dut;
    private readonly string _testPlant = "PlantA";
    private readonly int _no = 1;
    private readonly Guid _guid = Guid.NewGuid();

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new SWCR(_testPlant, _guid, _no);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_testPlant, _dut.Plant);
        Assert.AreEqual(_no, _dut.No);
        Assert.AreEqual(_guid, _dut.Guid);
    }
}
