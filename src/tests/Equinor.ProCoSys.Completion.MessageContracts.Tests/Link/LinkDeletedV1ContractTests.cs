using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.Link;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.Link;

[TestClass]
public class LinkDeletedV1ContractTests : ContractTestBase<ILinkDeletedV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "Plant", typeof(string) },
            { "Guid", typeof(Guid) },
            { "ParentGuid", typeof(Guid) },
            { "DeletedBy", typeof(User) },
            { "DeletedAtUtc", typeof(DateTime) },
            { "MessageId", typeof(Guid)}
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.Link");
}
