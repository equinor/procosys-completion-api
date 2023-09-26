using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.PunchItem;

[TestClass]
public class PunchItemCreatedV1ContractTests : ContractTestBase<IPunchItemCreatedV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "DisplayName", typeof(string) },
            { "Guid", typeof(Guid) },
            { "ProjectGuid", typeof(Guid) },
            { "ProjectName", typeof(string) },
            { "ProjectDescription", typeof(string) },
            { "ItemNo", typeof(int) },
            { "CreatedByOid", typeof(Guid) },
            { "CreatedAtUtc", typeof(DateTime) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange() => AssertNamespaceNotChanged();
}
