using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.Attachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.Attachment;

[TestClass]
public class AttachmentUpdatedV1ContractTests : ContractTestBase<IAttachmentUpdatedV1>
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
            { "FileName", typeof(string) },
            { "BlobPath", typeof(string) },
            { "ModifiedBy", typeof(IUser) },
            { "ModifiedAtUtc", typeof(DateTime) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.Attachment");
}
