using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.History;

[TestClass]
public class HistoryDeletedV1ContractTests : ContractTestBase<IHistoryItemDeletedV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "Plant", typeof(string) },
            { "DisplayName", typeof(string) },
            { "Guid", typeof(Guid) },
            { "ParentGuid", typeof(Guid?) },
            { "EventBy", typeof(User) },
            { "EventAtUtc", typeof(DateTime) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.History");
}
