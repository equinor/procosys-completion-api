using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.PunchItem;

[TestClass]
public class PunchItemUpdatedV1ContractTests : ContractTestBase<IPunchItemUpdatedV1>
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
            { "ClearedByOid", typeof(Guid?) },
            { "ClearedAtUtc", typeof(DateTime?) },
            { "RejectedByOid", typeof(Guid?) },
            { "RejectedAtUtc", typeof(DateTime?) },
            { "VerifiedByOid", typeof(Guid?) },
            { "VerifiedAtUtc", typeof(DateTime?) },
            { "ModifiedByOid", typeof(Guid) },
            { "ModifiedAtUtc", typeof(DateTime) },
            { "Changes", typeof(List<IProperty>) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.PunchItem");
}
