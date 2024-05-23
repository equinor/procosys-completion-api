using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.History;

[TestClass]
public class IChangedPropertyContractTests
{
    [TestMethod]
    public void IProperty_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "Name", typeof(string) },
            { "OldValue", typeof(object) },
            { "NewValue", typeof(object) },
            { "ValueDisplayType", typeof(ValueDisplayType) }
        };
        var actualProperties = typeof(IChangedProperty).GetProperties().ToDictionary(p => p.Name, p => p.PropertyType);

        // Act
        TestHelper.AssertPropertiesNotChanged(expectedProperties, actualProperties);
    }
}
