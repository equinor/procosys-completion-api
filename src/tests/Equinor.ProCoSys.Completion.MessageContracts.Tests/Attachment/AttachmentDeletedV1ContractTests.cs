using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.Attachment;

[TestClass]
public class AttachmentDeletedV1ContractTests : ContractTestBase<IAttachmentDeletedV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "DisplayName", typeof(string) },
            { "Guid", typeof(Guid) },
            { "SourceGuid", typeof(Guid) },
            { "DeletedByOid", typeof(Guid) },
            { "DeletedAtUtc", typeof(DateTime) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.Attachment");
}
