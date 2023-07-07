using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.PunchItem;

[TestClass]
public class PunchItemRejectedV1ContractTests : ContractTestBase<IPunchItemRejectedV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "DisplayName", typeof(string) },
            { "Guid", typeof(Guid) },
            { "ModifiedByOid", typeof(Guid) },
            { "ModifiedAtUtc", typeof(DateTime) },
            { "RejectedByOid", typeof(Guid) },
            { "RejectedAtUtc", typeof(DateTime) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange() => AssertNamespaceNotChanged();
}
