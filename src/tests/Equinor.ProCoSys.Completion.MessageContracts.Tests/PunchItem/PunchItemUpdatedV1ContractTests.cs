using System;
using System.Collections.Generic;
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
            { "Plant", typeof(string) },
            { "Guid", typeof(Guid) },
            { "ProjectGuid", typeof(Guid) },
            { "CheckListGuid", typeof(Guid) },
            { "ProjectName", typeof(string) },
            { "ProjectDescription", typeof(string) },
            { "Category", typeof(string) },
            { "ItemNo", typeof(long) },
            { "Description", typeof(string) },
            { "RaisedByOrgCode", typeof(string) },
            { "RaisedByOrgGuid", typeof(Guid) },
            { "ClearingByOrgCode", typeof(string) },
            { "ClearingByOrgGuid", typeof(Guid) },
            { "SortingCode", typeof(string) },
            { "SortingGuid", typeof(Guid?) },
            { "TypeCode", typeof(string) },
            { "TypeGuid", typeof(Guid?) },
            { "PriorityCode", typeof(string) },
            { "PriorityGuid", typeof(Guid?) },
            { "DueTimeUtc", typeof(DateTime?) },
            { "Estimate", typeof(int?) },
            { "ExternalItemNo", typeof(string) },
            { "MaterialRequired", typeof(bool) },
            { "MaterialETAUtc", typeof(DateTime?) },
            { "MaterialExternalNo", typeof(string) },
            { "WorkOrderNo", typeof(string) },
            { "WorkOrderGuid", typeof(Guid?) },
            { "OriginalWorkOrderNo", typeof(string) },
            { "OriginalWorkOrderGuid", typeof(Guid?) },
            { "DocumentNo", typeof(string) },
            { "DocumentGuid", typeof(Guid?) },
            { "SWCRNo", typeof(int?) },
            { "SWCRGuid", typeof(Guid?) },
            { "ActionBy", typeof(User) },
            { "ClearedBy", typeof(User) },
            { "ClearedAtUtc", typeof(DateTime?) },
            { "RejectedBy", typeof(User) },
            { "RejectedAtUtc", typeof(DateTime?) },
            { "VerifiedBy", typeof(User) },
            { "VerifiedAtUtc", typeof(DateTime?) },
            { "CreatedBy", typeof(User) },
            { "CreatedAtUtc", typeof(DateTime) },
            { "ModifiedBy", typeof(User) },
            { "ModifiedAtUtc", typeof(DateTime) },
            { "MessageId", typeof(Guid)},
            { "AffectsCompletionStatus", typeof(bool) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.PunchItem");
}
