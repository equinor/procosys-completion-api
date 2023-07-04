using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.MessageContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.MessageContracts;

public abstract class ContractTestBase<TContract> where TContract: IEventMessage
{
    private const string ExpectedNameSpace = "Equinor.ProCoSys.Completion.MessageContracts";

    [TestMethod]
    public abstract void Contract_InterfacePropertiesAndMethods_DoNotChange();
    [TestMethod]
    public abstract void Contract_Namespace_DoNotChange();

    /**
     * If this tests fails, its most likely because the versioning contract is breached. Consider creating a new version instead of
     * modifying the existing one.
     * If new properties are added to the interface (non breaking), this test should be updated with the new properties.
     * If existing properties are modified (breaking), a new version of the interface should be created.
     */
    protected void AssertContractNotBreached(Dictionary<string, Type> expectedProperties)
    {
        var contractToTestType = typeof(TContract);

        // Act
        var actualProperties = contractToTestType.GetProperties()
            .ToDictionary(p => p.Name, p => p.PropertyType);

        // Assert
        CollectionAssert.AreEquivalent(expectedProperties.Keys, actualProperties.Keys,
            "The number expected properties does not match number of interface properties, " +
            "test needs to be updated if the change is non breaking(non required property added)");

        foreach (var expectedProperty in expectedProperties)
        {
            Assert.AreEqual(expectedProperty.Value, actualProperties[expectedProperty.Key], "Property type mismatch. " +
                "Consider creating a new version instead of modifying the existing one.");
        }
    }

    /**
     * If this test fails, its mostly because the namespace of contract is other than Equinor.ProCoSys.Completion.MessageContracts
     * See adr 0004
     */
    protected void AssertNamespaceNotChanged()
    {
        var contractToTestType = typeof(TContract);

        // Assert 
        Assert.AreEqual(ExpectedNameSpace, contractToTestType.Namespace);
    }
}
