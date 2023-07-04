using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.MessageContracts.Punch;

[TestClass]
public class PunchCreatedV1ContractTests : ContractTestBase<IPunchCreatedV1>
{
    [TestMethod]
    public override void Contract_InterfacePropertiesAndMethods_DoNotChange()
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

        // Act
        AssertContractNotBreached(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange() => AssertNamespaceNotChanged();
}
