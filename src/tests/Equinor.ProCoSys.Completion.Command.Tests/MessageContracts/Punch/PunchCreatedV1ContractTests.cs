using System;
using System.Collections.Generic;
using Equinor.ProCoSys.MessageContracts.Punch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.MessageContracts.Punch;

[TestClass]
public class PunchCreatedV1ContractTests : ContractTestBase<IPunchCreatedV1>
{
    [TestMethod]
    public void IPunchCreatedV1_InterfacePropertiesAndMethods_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "Guid", typeof(Guid) },
            { "ProjectGuid", typeof(Guid) },
            { "ItemNo", typeof(string) },
            { "CreatedByOid", typeof(Guid) },
            { "CreatedAtUtc", typeof(DateTime) }
        };

        AssertContractNotBreached(expectedProperties);
    }
}
