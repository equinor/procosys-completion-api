using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Command.MessageContracts.Punch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.MessageContracts;

[TestClass]
public class PunchCreatedV1Tests
{
    /**
     * If this tests fails, its most likely because the versioning contract is breached. Consider creating a new version instead of
     * modifying the existing one.
     * If new properties are added to the interface (non breaking), this test should be updated with the new properties.
     * If existing properties are modified (breaking), a new version of the interface should be created.
     */
    
    [TestMethod]
    public void IPunchCreatedV1_InterfacePropertiesAndMethods_DoNotChange()
    {
        // Arrange
        var interfaceType = typeof(IPunchCreatedV1);
        var expectedProperties = new Dictionary<string, Type>
        {
            { "Guid", typeof(Guid) },
            { "ProjectGuid", typeof(Guid) },
            { "ItemNo", typeof(string) },
            { "CreatedByOid", typeof(Guid) },
            { "CreatedAtUtc", typeof(DateTime) }
        };
        
        // Act
        var actualProperties = interfaceType.GetProperties()
            .ToDictionary(p => p.Name, p => p.PropertyType);
        
        // Assert
        
        CollectionAssert.AreEquivalent(expectedProperties.Keys, actualProperties.Keys, 
            "The number expected properties does not match number of interface properties, " +
            "test needs to be updated if the change is non breaking(non required property added)");
        foreach (var expectedProperty in expectedProperties)
        {
            Assert.AreEqual(expectedProperty.Value, actualProperties[expectedProperty.Key],"Property type mismatch. " +
                "Consider creating a new version instead of modifying the existing one.");
            
        }
    }
}
