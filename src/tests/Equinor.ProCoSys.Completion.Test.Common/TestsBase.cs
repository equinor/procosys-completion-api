using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Test.Common;

[TestClass]
public abstract class TestsBase
{
    protected static string TestPlantA = "PCS$PlantA";
    protected static string TestPlantB = "PCS$PlantB";
    protected static string TestPlantWithoutData = "PCS$EmptyPlant";

    protected IUnitOfWork _unitOfWorkMock;
    protected IPlantProvider _plantProviderMock;
    protected ManualTimeProvider _timeProvider;
    protected DateTime _utcNow;
    protected Person _person;

    [TestInitialize]
    public void BaseSetup()
    {
        _person = new Person(Guid.NewGuid(), "F", "L", "U", "@", false);
        _person.SetProtectedIdForTesting(3);
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant.Returns(TestPlantA);
            
        _utcNow = new DateTime(2020, 1, 1, 1, 1, 1, DateTimeKind.Utc);
        _timeProvider = new ManualTimeProvider(_utcNow);
        TimeService.SetProvider(_timeProvider);
    }

    protected void AssertPerson(IProperty property, User value)
    {
        Assert.IsNotNull(property);
        var user = property.Value as User;
        Assert.IsNotNull(user);
        Assert.AreEqual(value.Oid, user.Oid);
        Assert.AreEqual(value.FullName, user.FullName);
    }

    protected void AssertProperty(IProperty property, object value)
    {
        Assert.IsNotNull(property);
        Assert.IsNotNull(value);
        Assert.AreEqual(value, property.Value);
    }

    protected void AssertChange(IChangedProperty change, object oldValue, object newValue)
    {
        Assert.IsNotNull(change);
        Assert.AreEqual(oldValue, change.OldValue);
        Assert.AreEqual(newValue, change.NewValue);
    }

    protected void AssertPersonChange(IChangedProperty change, User oldValue, User newValue)
    {
        Assert.IsNotNull(change);
        if (change.OldValue is null)
        {
            Assert.IsNull(oldValue);
        }
        else
        {
            var user = change.OldValue as User;
            Assert.IsNotNull(user);
            Assert.AreEqual(oldValue.Oid, user.Oid);
            Assert.AreEqual(oldValue.FullName, user.FullName);
        }
        if (change.NewValue is null)
        {
            Assert.IsNull(newValue);
        }
        else
        {
            var user = change.NewValue as User;
            Assert.IsNotNull(user);
            Assert.AreEqual(newValue.Oid, user.Oid);
            Assert.AreEqual(newValue.FullName, user.FullName);
        }
    }

    protected void AssertRequiredProperties(PunchItem punchItem, IPunchItem integrationEvent)
    {
        Assert.AreEqual(punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(punchItem.Project.Guid, integrationEvent.ProjectGuid);
        Assert.AreEqual(punchItem.Project.Description, integrationEvent.ProjectDescription);
        Assert.AreEqual(punchItem.Project.Name, integrationEvent.ProjectName);
        Assert.AreEqual(punchItem.CheckListGuid, integrationEvent.CheckListGuid);
        Assert.AreEqual(punchItem.Category.ToString(), integrationEvent.Category);
        Assert.AreEqual(punchItem.ItemNo, integrationEvent.ItemNo);
        Assert.AreEqual(punchItem.RaisedByOrg.Code, integrationEvent.RaisedByOrgCode);
        Assert.AreEqual(punchItem.RaisedByOrg.Guid, integrationEvent.RaisedByOrgGuid);
        Assert.AreEqual(punchItem.ClearingByOrg.Code, integrationEvent.ClearingByOrgCode);
        Assert.AreEqual(punchItem.ClearingByOrg.Guid, integrationEvent.ClearingByOrgGuid);
        Assert.AreEqual(punchItem.CreatedAtUtc, integrationEvent.CreatedAtUtc);
        Assert.AreEqual(punchItem.CreatedBy.Guid, integrationEvent.CreatedBy.Oid);
        Assert.AreEqual(punchItem.CreatedBy.GetFullName(), integrationEvent.CreatedBy.FullName);
    }

    protected void AssertIsVerified(PunchItem punchItem, Person person, IPunchItem integrationEvent)
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

    protected void AssertIsRejected(PunchItem punchItem, Person person, IPunchItem integrationEvent)
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

    protected void AssertIsCleared(PunchItem punchItem, Person person, IPunchItem integrationEvent)
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
        Assert.AreEqual(punchItem.Sorting!.Guid, integrationEvent.SortingGuid);
        Assert.AreEqual(punchItem.Type!.Code, integrationEvent.TypeCode);
        Assert.AreEqual(punchItem.Type!.Guid, integrationEvent.TypeGuid);
        Assert.AreEqual(punchItem.Priority!.Code, integrationEvent.PriorityCode);
        Assert.AreEqual(punchItem.Priority!.Guid, integrationEvent.PriorityGuid);
        Assert.AreEqual(punchItem.DueTimeUtc, integrationEvent.DueTimeUtc);
        Assert.AreEqual(punchItem.Estimate, integrationEvent.Estimate);
        Assert.AreEqual(punchItem.ExternalItemNo, integrationEvent.ExternalItemNo);
        Assert.AreEqual(punchItem.MaterialRequired, integrationEvent.MaterialRequired);
        Assert.AreEqual(punchItem.MaterialETAUtc, integrationEvent.MaterialETAUtc);
        Assert.AreEqual(punchItem.MaterialExternalNo, integrationEvent.MaterialExternalNo);
        Assert.AreEqual(punchItem.WorkOrder!.No, integrationEvent.WorkOrderNo);
        Assert.AreEqual(punchItem.WorkOrder!.Guid, integrationEvent.WorkOrderGuid);
        Assert.AreEqual(punchItem.OriginalWorkOrder!.No, integrationEvent.OriginalWorkOrderNo);
        Assert.AreEqual(punchItem.OriginalWorkOrder!.Guid, integrationEvent.OriginalWorkOrderGuid);
        Assert.AreEqual(punchItem.Document!.No, integrationEvent.DocumentNo);
        Assert.AreEqual(punchItem.Document!.Guid, integrationEvent.DocumentGuid);
        Assert.AreEqual(punchItem.SWCR!.No, integrationEvent.SWCRNo);
        Assert.AreEqual(punchItem.SWCR!.Guid, integrationEvent.SWCRGuid);
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
        // ... use ModifiedBy/ModifiedAtUtc which is set when saving a deletion
        Assert.AreEqual(punchItem.ModifiedAtUtc, integrationEvent.DeletedAtUtc);
        Assert.AreEqual(punchItem.ModifiedBy!.Guid, integrationEvent.DeletedBy.Oid);
        Assert.AreEqual(punchItem.ModifiedBy!.GetFullName(), integrationEvent.DeletedBy.FullName);
    }

    protected void AssertHistoryCreatedIntegrationEvent(
        HistoryCreatedIntegrationEvent historyEvent, 
        string plant, 
        string displayName,
        Guid parentGuid,
        IHaveGuid guidEntity,
        ICreationAuditable creationAuditableEntity)
    {
        Assert.AreEqual(plant, historyEvent.Plant);
        Assert.AreEqual(displayName, historyEvent.DisplayName);
        Assert.AreEqual(guidEntity.Guid, historyEvent.Guid);
        Assert.AreEqual(parentGuid, historyEvent.ParentGuid);
        Assert.AreEqual(creationAuditableEntity.CreatedBy.Guid, historyEvent.EventBy.Oid);
        Assert.AreEqual(creationAuditableEntity.CreatedBy.GetFullName(), historyEvent.EventBy.FullName);
        Assert.AreEqual(creationAuditableEntity.CreatedAtUtc, historyEvent.EventAtUtc);
    }

    protected void AssertHistoryUpdatedIntegrationEvent(
        HistoryUpdatedIntegrationEvent historyEvent, 
        string plant,
        string displayName,
        IHaveGuid guidEntity,
        IModificationAuditable modificationAuditableEntity)
    {
        Assert.IsNotNull(historyEvent);
        Assert.AreEqual(displayName, historyEvent.DisplayName);
        Assert.AreEqual(plant, historyEvent.Plant);
        Assert.AreEqual(guidEntity.Guid, historyEvent.Guid);
        Assert.AreEqual(modificationAuditableEntity.ModifiedAtUtc, historyEvent.EventAtUtc);
        Assert.AreEqual(modificationAuditableEntity.ModifiedBy!.Guid, historyEvent.EventBy.Oid);
        Assert.AreEqual(modificationAuditableEntity.ModifiedBy!.GetFullName(), historyEvent.EventBy.FullName);
    }

    protected void AssertHistoryDeletedIntegrationEvent(
        HistoryDeletedIntegrationEvent historyEvent,
        string plant,
        string displayName,
        Guid parentGuid,
        IHaveGuid guidEntity,
        IModificationAuditable modificationAuditableEntity)
    {
        Assert.IsNotNull(historyEvent);
        Assert.AreEqual(displayName, historyEvent.DisplayName);
        Assert.AreEqual(plant, historyEvent.Plant);
        Assert.AreEqual(guidEntity.Guid, historyEvent.Guid);

        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... use ModifiedBy/ModifiedAtUtc which is set when saving a deletion
        Assert.AreEqual(modificationAuditableEntity.ModifiedAtUtc, historyEvent.EventAtUtc);
        Assert.AreEqual(modificationAuditableEntity.ModifiedBy!.Guid, historyEvent.EventBy.Oid);
        Assert.AreEqual(modificationAuditableEntity.ModifiedBy!.GetFullName(), historyEvent.EventBy.FullName);
    }
}
