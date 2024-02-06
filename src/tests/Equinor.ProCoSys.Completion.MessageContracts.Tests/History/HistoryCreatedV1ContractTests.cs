using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.History;

[TestClass]
public class HistoryCreatedV1ContractTests : ContractTestBase<IHistoryItemCreatedV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "DisplayName", typeof(string) },
            { "Guid", typeof(Guid) },
            { "ParentGuid", typeof(Guid?) },
            { "EventBy", typeof(User) },
            { "EventAtUtc", typeof(DateTime) },
            { "Properties", typeof(List<IProperty>) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.History");
}
