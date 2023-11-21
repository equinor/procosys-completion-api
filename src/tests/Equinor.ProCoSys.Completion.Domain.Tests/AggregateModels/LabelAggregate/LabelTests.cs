using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
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
        Assert.IsNotNull(_dut.Hosts);
        Assert.AreEqual(0, _dut.Hosts.Count);
    }
}
