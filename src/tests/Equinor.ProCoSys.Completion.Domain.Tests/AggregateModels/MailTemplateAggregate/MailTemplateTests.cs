using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.MailTemplateAggregate;

[TestClass]
public class MailTemplateTests
{
    private MailTemplate _dut;
    private readonly string _code = "X";
    private readonly string _subject = "X Subject";
    private readonly string _body = "X Body";

    [TestInitialize]
    public void Setup() => _dut = new MailTemplate(_code, _subject, _body);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_code, _dut.Code);
        Assert.AreEqual(_subject, _dut.Subject);
        Assert.AreEqual(_body, _dut.Body);
    }

    [TestMethod]
    public void IsGlobal_ShouldBeTrue_WhenPlantIsNull()
    {
        // Act
        var result = _dut.IsGlobal();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsGlobal_ShouldBeFalse_WhenPlantHasValue()
    {
        // Arrange
        _dut.Plant = "p";

        // Act
        var result = _dut.IsGlobal();

        // Assert
        Assert.IsFalse(result);
    }
}
