using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests;

[TestClass]
public class OperationListExtensionTests
{
    [TestMethod]
    public void GetReplaceOperation_ShouldReturnOperation_WhenExists()
    {
        // Arrange
        var propName = nameof(MyClass.Str);
        var path = $"/{propName}";
        var value = "blah";
        var dut = new List<Operation<MyClass>> { new("replace", path, null, value) };

        // Act 
        var result = dut.GetReplaceOperation(propName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(OperationType.Replace, result.OperationType);
        Assert.AreEqual(path, result.path);
        Assert.AreEqual(value, result.value);
    }

    [TestMethod]
    public void GetReplaceOperation_ShouldReturnNull_WhenOperationIsOtherThanReplace()
    {
        // Arrange
        var propName = nameof(MyClass.Str);
        var path = $"/{propName}";
        var dut = new List<Operation<MyClass>> { new("remove", path, null) };

        // Act 
        var result = dut.GetReplaceOperation(propName);

        // Assert
        Assert.IsNull(result);
    }
    [TestMethod]
    public void GetReplaceOperation_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var propName = nameof(MyClass.Str);
        var path = $"/{propName}";
        var value = "blah";
        var dut = new List<Operation<MyClass>> { new("replace", path, null, value) };

        // Act 
        var result = dut.GetReplaceOperation("UnknownProp");

        // Assert
        Assert.IsNull(result);
    }


    // ReSharper disable once ClassNeverInstantiated.Local
    private class MyClass
    {
        public string Str { get; set; }
    }
}
