using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests;

public abstract class ContractTestBase<TContract> where TContract: IIntegrationEvent
{
    [TestMethod]
    public abstract void Contract_Interface_DoNotChange();
    [TestMethod]
    public abstract void Contract_Namespace_DoNotChange();

    /**
     * If this tests fails, its most likely because the versioning contract is breached. Consider creating a new version instead of
     * modifying the existing one.
     * If new properties are added to the interface (non breaking), this test should be updated with the new properties.
     * If existing properties are modified (breaking), a new version of the interface should be created.
     */
    protected void AssertPropertiesNotChanged(Dictionary<string, Type> expectedProperties)
    {
        var actualProperties = GetAllProperties();

        TestHelper.AssertPropertiesNotChanged(expectedProperties, actualProperties);
    }

    /**
     * If this test fails, its mostly because the namespace of versioning contract had been moved.
     * This is a breaking change for message receiver, since full namespace are used when subscribing with mass transit
     */
    protected void AssertNamespaceNotChanged(string expectedNameSpace)
    {
        var contractToTestType = typeof(TContract);

        // Assert 
        Assert.AreEqual(expectedNameSpace, contractToTestType.Namespace);
    }

    private static Dictionary<string, Type> GetAllProperties()
    {
        var contractToTestType = typeof(TContract);

        var actualProperties = contractToTestType.GetProperties().ToDictionary(p => p.Name, p => p.PropertyType);

        var interfaces = contractToTestType.GetInterfaces();
        foreach (var @interface in interfaces)
        {
            var props = @interface.GetProperties();
            foreach (var prop in props)
            {
                actualProperties.Add(prop.Name, prop.PropertyType);
            }
        }
        return actualProperties;
    }
}
