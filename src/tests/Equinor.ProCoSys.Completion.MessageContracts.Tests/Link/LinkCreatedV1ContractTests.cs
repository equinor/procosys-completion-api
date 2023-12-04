using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.Link;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.Link;

[TestClass]
public class LinkCreatedV1ContractTests : ContractTestBase<ILinkCreatedV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "DisplayName", typeof(string) },
            { "Guid", typeof(Guid) },
            { "ParentGuid", typeof(Guid) },
            { "ParentType", typeof(string) },
            { "Title", typeof(string) },
            { "Url", typeof(string) },
            { "CreatedBy", typeof(User) },
            { "CreatedAtUtc", typeof(DateTime) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.Link");
}
