using System;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.PunchItemEvents;

public class PunchItemIntegrationEventTestsBase
{
    protected string _testPlant = "X";
    protected DateTime _now = new(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    protected Person _person;
    protected PunchItem _punchItem;
    protected Project _project;

    [TestInitialize]
    public void SetupBase()
    {
        TimeService.SetProvider(new ManualTimeProvider(_now));
        _person = new Person(Guid.NewGuid(), "Yo", "Da", "YD", "@", false);
        _person.SetProtectedIdForTesting(3);

        _project = new Project(_testPlant, Guid.NewGuid(), null!, null!, DateTime.Now);
        var raisedByOrg = new LibraryItem(_testPlant, Guid.NewGuid(), "RC", "RD", LibraryType.COMPLETION_ORGANIZATION);
        var clearingByOrg = new LibraryItem(_testPlant, Guid.NewGuid(), "CC", "CD", LibraryType.COMPLETION_ORGANIZATION);
        _punchItem = new PunchItem(_testPlant, _project, Guid.NewGuid(), Category.PB, "Desc", raisedByOrg, clearingByOrg);
        _punchItem.SetCreated(_person);
    }

    protected void AssertRequiredProperties(PunchItem punchItem, IPunchItem integrationEvent)
    {
        Assert.AreEqual(punchItem.Project.Guid, integrationEvent.ProjectGuid);
        Assert.AreEqual(punchItem.Project.Description, integrationEvent.ProjectDescription);
        Assert.AreEqual(punchItem.Project.Name, integrationEvent.ProjectName);
        Assert.AreEqual(punchItem.Category.ToString(), integrationEvent.Category);
        Assert.AreEqual(punchItem.ItemNo, integrationEvent.ItemNo);
        Assert.AreEqual(punchItem.RaisedByOrg.Code, integrationEvent.RaisedByOrgCode);
        Assert.AreEqual(punchItem.ClearingByOrg.Code, integrationEvent.ClearingByOrgCode);
        Assert.AreEqual(punchItem.CreatedAtUtc, integrationEvent.CreatedAtUtc);
        Assert.AreEqual(punchItem.CreatedBy.Guid, integrationEvent.CreatedBy.Oid);
        Assert.AreEqual(punchItem.CreatedBy.GetFullName(), integrationEvent.CreatedBy.FullName);
    }

    protected void FillOptionalProperties(PunchItem punchItem)
    {
        punchItem.SetSorting(new LibraryItem(_testPlant, Guid.NewGuid(), "SortC", "SortD", LibraryType.PUNCHLIST_SORTING));
        punchItem.SetPriority(new LibraryItem(_testPlant, Guid.NewGuid(), "PriC", "PriD", LibraryType.PUNCHLIST_PRIORITY));
        punchItem.SetType(new LibraryItem(_testPlant, Guid.NewGuid(), "TypC", "TypD", LibraryType.PUNCHLIST_TYPE));
        punchItem.DueTimeUtc = DateTime.UtcNow;
        punchItem.Estimate = 100;
        punchItem.ExternalItemNo = "100";
        punchItem.MaterialRequired = true;
        punchItem.MaterialETAUtc = DateTime.UtcNow.AddDays(2);
        punchItem.MaterialExternalNo = "120";
        punchItem.SetWorkOrder(new WorkOrder(_testPlant, Guid.NewGuid(), "WO1"));
        punchItem.SetOriginalWorkOrder(new WorkOrder(_testPlant, Guid.NewGuid(), "WO2"));
        punchItem.SetDocument(new Document(_testPlant, Guid.NewGuid(), "Doc"));
        punchItem.SetSWCR(new SWCR(_testPlant, Guid.NewGuid(), 7));
        punchItem.SetActionBy(new Person(Guid.NewGuid(), null!, null!, null!, null!, false));
    }

    protected void AssertIsVerified(PunchItem punchItem, Person person, PunchItemUpdatedIntegrationEvent integrationEvent)
    {
        Assert.IsNotNull(integrationEvent.VerifiedAtUtc);
        Assert.AreEqual(punchItem.VerifiedAtUtc, integrationEvent.VerifiedAtUtc);
        Assert.IsNotNull(integrationEvent.VerifiedBy);
        Assert.AreEqual(person.Guid, integrationEvent.VerifiedBy.Oid);
        Assert.AreEqual(person.GetFullName(), integrationEvent.VerifiedBy.FullName);
    }

    protected void AssertNotVerified(IPunchItem integrationEvent)
    {
        Assert.IsNull(integrationEvent.VerifiedBy);
        Assert.IsNull(integrationEvent.VerifiedAtUtc);
    }

    protected void AssertIsRejected(PunchItem punchItem, Person person, PunchItemUpdatedIntegrationEvent integrationEvent)
    {
        Assert.IsNotNull(integrationEvent.RejectedAtUtc);
        Assert.AreEqual(punchItem.RejectedAtUtc, integrationEvent.RejectedAtUtc);
        Assert.IsNotNull(integrationEvent.RejectedBy);
        Assert.AreEqual(person.Guid, integrationEvent.RejectedBy.Oid);
        Assert.AreEqual(person.GetFullName(), integrationEvent.RejectedBy.FullName);
    }

    protected void AssertNotRejected(IPunchItem integrationEvent)
    {
        Assert.IsNull(integrationEvent.RejectedBy);
        Assert.IsNull(integrationEvent.RejectedAtUtc);
    }

    protected void AssertIsCleared(PunchItem punchItem, Person person, PunchItemUpdatedIntegrationEvent integrationEvent)
    {
        Assert.AreEqual(punchItem.ClearedAtUtc, integrationEvent.ClearedAtUtc);
        Assert.IsNotNull(integrationEvent.ClearedBy);
        Assert.AreEqual(person.Guid, integrationEvent.ClearedBy.Oid);
        Assert.AreEqual(person.GetFullName(), integrationEvent.ClearedBy.FullName);
    }

    protected void AssertNotCleared(IPunchItem integrationEvent)
    {
        Assert.IsNull(integrationEvent.ClearedBy);
        Assert.IsNull(integrationEvent.ClearedAtUtc);
    }

    protected void AssertOptionalPropertiesIsNull(IPunchItem integrationEvent)
    {
        Assert.IsNull(integrationEvent.SortingCode);
        Assert.IsNull(integrationEvent.TypeCode);
        Assert.IsNull(integrationEvent.PriorityCode);
        Assert.IsNull(integrationEvent.DueTimeUtc);
        Assert.IsNull(integrationEvent.Estimate);
        Assert.IsNull(integrationEvent.ExternalItemNo);
        Assert.IsFalse(integrationEvent.MaterialRequired);
        Assert.IsNull(integrationEvent.MaterialETAUtc);
        Assert.IsNull(integrationEvent.MaterialExternalNo);
        Assert.IsNull(integrationEvent.WorkOrderNo);
        Assert.IsNull(integrationEvent.OriginalWorkOrderNo);
        Assert.IsNull(integrationEvent.DocumentNo);
        Assert.IsNull(integrationEvent.SWCRNo);
        Assert.IsNull(integrationEvent.ActionBy);
    }

    protected void AssertOptionalProperties(PunchItem punchItem, IPunchItem integrationEvent)
    {
        Assert.AreEqual(punchItem.Sorting!.Code, integrationEvent.SortingCode);
        Assert.AreEqual(punchItem.Type!.Code, integrationEvent.TypeCode);
        Assert.AreEqual(punchItem.Priority!.Code, integrationEvent.PriorityCode);
        Assert.AreEqual(punchItem.DueTimeUtc, integrationEvent.DueTimeUtc);
        Assert.AreEqual(punchItem.Estimate, integrationEvent.Estimate);
        Assert.AreEqual(punchItem.ExternalItemNo, integrationEvent.ExternalItemNo);
        Assert.AreEqual(punchItem.MaterialRequired, integrationEvent.MaterialRequired);
        Assert.AreEqual(punchItem.MaterialETAUtc, integrationEvent.MaterialETAUtc);
        Assert.AreEqual(punchItem.MaterialExternalNo, integrationEvent.MaterialExternalNo);
        Assert.AreEqual(punchItem.WorkOrder!.No, integrationEvent.WorkOrderNo);
        Assert.AreEqual(punchItem.OriginalWorkOrder!.No, integrationEvent.OriginalWorkOrderNo);
        Assert.AreEqual(punchItem.Document!.No, integrationEvent.DocumentNo);
        Assert.AreEqual(punchItem.SWCR!.No, integrationEvent.SWCRNo);
        Assert.IsNotNull(integrationEvent.ActionBy);
        Assert.AreEqual(punchItem.ActionBy!.Guid, integrationEvent.ActionBy.Oid);
        Assert.AreEqual(punchItem.ActionBy!.GetFullName(), integrationEvent.ActionBy.FullName);
    }

    protected void AssertModifiedBy(PunchItem punchItem, PunchItemUpdatedIntegrationEvent integrationEvent)
    {
        Assert.AreEqual(punchItem.ModifiedAtUtc, integrationEvent.ModifiedAtUtc);
        Assert.AreEqual(punchItem.ModifiedBy!.Guid, integrationEvent.ModifiedBy.Oid);
        Assert.AreEqual(punchItem.ModifiedBy!.GetFullName(), integrationEvent.ModifiedBy.FullName);
    }

    protected void AssertDeletedBy(PunchItem punchItem, PunchItemDeletedIntegrationEvent integrationEvent)
    {
        // Our domain entities don't have DeletedByOid / DeletedAtUtc ...
        // ... use ModifiedBy/ModifiedAtUtc which is set when saving a delete
        Assert.AreEqual(punchItem.ModifiedAtUtc, integrationEvent.DeletedAtUtc);
        Assert.AreEqual(punchItem.ModifiedBy!.Guid, integrationEvent.DeletedBy.Oid);
        Assert.AreEqual(punchItem.ModifiedBy!.GetFullName(), integrationEvent.DeletedBy.FullName);
    }
}
