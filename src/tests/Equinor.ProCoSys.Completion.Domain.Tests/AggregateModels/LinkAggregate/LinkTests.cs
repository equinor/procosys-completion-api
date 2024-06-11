using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.LinkAggregate;

[TestClass]
public class LinkTests : IModificationAuditableTests
{
    private Link _dut;
    private readonly string _parentType = "X";
    private readonly Guid _parentGuid = Guid.NewGuid();
    private readonly string _title = "A";
    private readonly string _url = "Desc A";
    private readonly Guid _proCoSysGuid = Guid.NewGuid();

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Link(_parentType, _parentGuid, _title, _url, _proCoSysGuid);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_title, _dut.Title);
        Assert.AreEqual(_url, _dut.Url);
        Assert.AreEqual(_parentType, _dut.ParentType);
        Assert.AreEqual(_parentGuid, _dut.ParentGuid);
        Assert.AreNotEqual(_parentGuid, _dut.Guid);
        Assert.AreNotEqual(Guid.Empty, _dut.Guid);
    }

    [TestMethod]
    public void Constructor_ShouldSetProCoSysGuid_WhenGiven() =>
        // Assert
        Assert.AreEqual(_proCoSysGuid, _dut.Guid);
}
