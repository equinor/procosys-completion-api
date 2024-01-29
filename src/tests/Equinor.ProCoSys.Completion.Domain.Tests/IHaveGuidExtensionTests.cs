using System;
using System.Dynamic;
using Equinor.ProCoSys.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests;

[TestClass]
public class IHaveGuidExtensionTests
{
    private readonly TestableEntity _dut = new();

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
        Assert.AreEqual("Test", emailContext.MyProp);
    }

    private class TestableEntity : IHaveGuid
    {
        public Guid Guid { get; } = Guid.NewGuid();
        // ReSharper disable once UnusedMember.Local
        public string GetContextType() => nameof(TestableEntity);
        public dynamic GetEmailContext()
        {
            dynamic expandoObject = new ExpandoObject();
            expandoObject.Entity = this;
            expandoObject.MyProp = "Test";
            return expandoObject;
        }
    }
}
