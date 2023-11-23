using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
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
            { "ParentGuid", typeof(Guid) },
            { "ProjectGuid", typeof(Guid) },
            { "CheckListGuid", typeof(Guid) },
            { "ProjectName", typeof(string) },
            { "ProjectDescription", typeof(string) },
            { "Category", typeof(string) },
            { "ItemNo", typeof(int) },
            { "Description", typeof(string) },
            { "RaisedByOrgCode", typeof(string) },
            { "ClearingByOrgCode", typeof(string) },
            { "SortingCode", typeof(string) },
            { "TypeCode", typeof(string) },
            { "PriorityCode", typeof(string) },
            { "DueTimeUtc", typeof(DateTime?) },
            { "Estimate", typeof(int?) },
            { "ExternalItemNo", typeof(string) },
            { "MaterialRequired", typeof(bool) },
            { "MaterialETAUtc", typeof(DateTime?) },
            { "MaterialExternalNo", typeof(string) },
            { "WorkOrderNo", typeof(string) },
            { "OriginalWorkOrderNo", typeof(string) },
            { "DocumentNo", typeof(string) },
            { "SWCRNo", typeof(int?) },
            { "ActionBy", typeof(IUser) },
            { "ClearedBy", typeof(IUser) },
            { "ClearedAtUtc", typeof(DateTime?) },
            { "RejectedBy", typeof(IUser) },
            { "RejectedAtUtc", typeof(DateTime?) },
            { "VerifiedBy", typeof(IUser) },
            { "VerifiedAtUtc", typeof(DateTime?) },
            { "CreatedBy", typeof(IUser) },
            { "CreatedAtUtc", typeof(DateTime) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.PunchItem");
}
