using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests;

[TestClass]
public class IUserContractTests
{
    [TestMethod]
    public void IUser_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "Oid", typeof(Guid) },
            { "FullName", typeof(string) }
        };
        var actualProperties = typeof(IUser).GetProperties().ToDictionary(p => p.Name, p => p.PropertyType);

        // Act
        TestHelper.AssertPropertiesNotChanged(expectedProperties, actualProperties);
    }
}
