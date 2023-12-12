using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.LabelEntityAggregate;

[TestClass]
public class LabelEntityTests : IModificationAuditableTests
{
    private LabelEntity _dut;
    private readonly EntityTypeWithLabel _entityType = EntityTypeWithLabel.PunchComment;

    protected override ICreationAuditable GetCreationAuditable() => _dut;
    protected override IModificationAuditable GetModificationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new LabelEntity(_entityType);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_entityType, _dut.EntityType);
        Assert.IsNotNull(_dut.Labels);
        Assert.AreEqual(0, _dut.Labels.Count);
    }
}
