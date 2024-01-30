using System;
using Equinor.ProCoSys.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests;

[TestClass]
public class IHaveGuidExtensionTests
{
    private const string _myPropValue = "Test";
    private readonly TestableEntity _dut = new(_myPropValue);

    [TestMethod]
    public void GetContextType_ShouldReturnNameOfTestableEntity()
    {
        // Act
        var contextType = _dut.GetContextName();

        // Assert
        Assert.AreEqual("TestableEntity", contextType);
    }

    [TestMethod]
    public void GetEmailContext_ShouldReturnDynamicContext_WithTheTestableEntityAsEntity()
    {
        // Act
        var emailContext = _dut.GetEmailContext();

        // Assert
        Assert.AreEqual(_dut, emailContext.Entity);
        var testableEntity = emailContext.Entity as TestableEntity;
        Assert.IsNotNull(testableEntity);
        Assert.AreEqual(_myPropValue, testableEntity.MyProp);
    }

    private class TestableEntity(string myProp) : IHaveGuid
    {
        public Guid Guid { get; } = Guid.NewGuid();
        public string MyProp { get; } = myProp;
    }
}
