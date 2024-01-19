using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

[TestClass]
public class PropertyMappingTests
{
    private readonly Guid _expectedGuid1Value = new();
    private readonly Guid _expectedGuid2Value = new();
    private SourceTestObject _testObject;
    private readonly string _expectedPropertyInTheObject = "TestGuid";
    private readonly string _expectedNestedPropertyInTheObject = "NestedObject.Guid";
    private readonly string _expectedNonExistingProperty = "NonExistingProperty";

    [TestInitialize]
    public void Setup() => _testObject = new SourceTestObject(null, _expectedGuid1Value, null, null, null, true, null, new NestedSourceTestObject(_expectedGuid2Value), null, null, null, null);

    [TestMethod]
    public void GetSourcePropertyValue_ShouldReturnCorrectValue_WhenPropertyExist()
    {
        // Act
        var actualValue = PropertyMapping.GetSourcePropertyValue(_expectedPropertyInTheObject, _testObject);

        // Assert
        Assert.AreEqual(_expectedGuid1Value, actualValue);
    }

    [TestMethod]
    public void GetSourcePropertyValue_ShouldReturnCorrectNestedValue_WhenPropertyExist()
    {
        // Act
        var actualValue = PropertyMapping.GetSourcePropertyValue(_expectedNestedPropertyInTheObject, _testObject);

        // Assert
        Assert.AreEqual(_expectedGuid2Value, actualValue);
    }

    [TestMethod]
    public void GetSourcePropertyValue_ShouldReturnException_WhenPropertyDoesNotExist()
    {
        //Act 
        var exception = Assert.ThrowsException<Exception>(() => PropertyMapping.GetSourcePropertyValue(_expectedNonExistingProperty, _testObject));

        // Assert
        Assert.AreEqual($"A property in configuration is missing in source object: {_expectedNonExistingProperty}", exception.Message);
    }
}
