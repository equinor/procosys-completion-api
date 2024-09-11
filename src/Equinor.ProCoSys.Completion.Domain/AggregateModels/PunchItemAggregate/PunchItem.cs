using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Person = Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate.Person;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

public class PunchItem : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IHaveGuid
{
    private DateTime? _dueTimeUtc;
    private DateTime? _materialETAUtc;
    public const int IdentitySeed = 4000001;
    public const int ItemNoStartsAtt = 5000000;
    public const string PunchItemItemNoSequence = "SEQ_PUNCHITEM_ITEMNO";
    public const int DescriptionLengthMin = 1;
    public const int DescriptionLengthMax = 3500;
    public const int ExternalItemNoLengthMax = 100;
    public const int MaterialExternalNoLengthMax = 100;

#pragma warning disable CS8618
    protected PunchItem()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public PunchItem(
        string plant,
        Project project,
        Guid checkListGuid,
        Category category,
        string description,
        LibraryItem raisedByOrg,
        LibraryItem clearingByOrg,
        Guid? proCoSysGuid = null)
        : base(plant)
    {
        CheckListGuid = checkListGuid;
        Category = category;
        Description = description;
        Guid = proCoSysGuid ?? MassTransit.NewId.NextGuid();

        SetProject(plant, project);
        SetRaisedByOrg(raisedByOrg);
        SetClearingByOrg(clearingByOrg);
    }

    // private setters needed for Entity Framework
    public int ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    // Guid to CheckList in ProCoSys 4 owning the Punch. Will probably be an internal Id to Internal CheckList table when CheckList migrated to Completion
    public Guid CheckListGuid { get; private set; }
    public Category Category { get; set; }
    public long ItemNo { get; private set; }
    public string Description { get; set; }
    public LibraryItem RaisedByOrg { get; private set; } = null!;
    public int RaisedByOrgId { get; private set; }
    public LibraryItem ClearingByOrg { get; private set; } = null!;
    public int ClearingByOrgId { get; private set; }
    public LibraryItem? Sorting { get; private set; }
    public int? SortingId { get; private set; }
    public LibraryItem? Type { get; private set; }
    public int? TypeId { get; private set; }
    public LibraryItem? Priority { get; private set; }
    public int? PriorityId { get; private set; }

    public DateTime? DueTimeUtc
    {
        get => _dueTimeUtc;
        set
        {
            if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
            {
                throw new Exception($"{nameof(PunchItem)}.{nameof(DueTimeUtc)} must be UTC");
            }
            _dueTimeUtc = value;
        }
    }

    public int? Estimate { get; set; }
    public string? ExternalItemNo { get; set; }
    public bool MaterialRequired { get; set; }

    public DateTime? MaterialETAUtc
    {
        get => _materialETAUtc;
        set
        {
            if (value.HasValue && value.Value.Kind != DateTimeKind.Utc)
            {
                throw new Exception($"{nameof(PunchItem)}.{nameof(DueTimeUtc)} must be UTC");
            }
            _materialETAUtc = value;
        }
    }

    public string? MaterialExternalNo { get; set; }
    public WorkOrder? WorkOrder { get; private set; }
    public int? WorkOrderId { get; private set; }
    public WorkOrder? OriginalWorkOrder { get; private set; }
    public int? OriginalWorkOrderId { get; private set; }
    public Document? Document { get; private set; }
    public int? DocumentId { get; private set; }
    public SWCR? SWCR { get; private set; }
    public int? SWCRId { get; private set; }
    public Person? ActionBy { get; private set; }
    public int? ActionById { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public Guid Guid { get; private set; }
    public DateTime? ClearedAtUtc { get; private set; }
    public int? ClearedById { get; private set; }
    public Person? ClearedBy { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }
    public int? RejectedById { get; private set; }
    public Person? RejectedBy { get; private set; }
    public DateTime? VerifiedAtUtc { get; private set; }
    public int? VerifiedById { get; private set; }
    public Person? VerifiedBy { get; private set; }

    public bool IsCleared => ClearedAtUtc.HasValue;
    public bool IsVerified => VerifiedAtUtc.HasValue;
    public bool IsRejected => RejectedAtUtc.HasValue;

    public bool IsReadyToBeCleared => !IsCleared;
    public bool IsReadyToBeRejected => IsCleared && !IsVerified;
    public bool IsReadyToBeVerified => IsCleared && !IsVerified;
    public bool IsReadyToBeUncleared => IsCleared && !IsVerified;
    public bool IsReadyToBeUnverified => IsVerified;

    public void Clear(Person clearedBy)
    {
        if (!IsReadyToBeCleared)
        {
            throw new Exception($"{nameof(PunchItem)} can not be cleared");
        }
        ClearedAtUtc = TimeService.UtcNow;
        ClearedBy = clearedBy;
        ClearedById = clearedBy.Id;
        RejectedAtUtc = null;
        RejectedById = null;
    }

    public void Reject(Person rejectedBy)
    {
        if (!IsReadyToBeRejected)
        {
            throw new Exception($"{nameof(PunchItem)} can not be rejected");
        }
        RejectedAtUtc = TimeService.UtcNow;
        RejectedBy = rejectedBy;
        RejectedById = rejectedBy.Id;
        ClearedAtUtc = null;
        ClearedBy = null;
        ClearedById = null;
    }

    public void Verify(Person verifiedBy)
    {
        if (!IsReadyToBeVerified)
        {
            throw new Exception($"{nameof(PunchItem)} can not be verified");
        }
        VerifiedAtUtc = TimeService.UtcNow;
        VerifiedBy = verifiedBy;
        VerifiedById = verifiedBy.Id;
    }

    public void Unclear()
    {
        if (!IsReadyToBeUncleared)
        {
            throw new Exception($"{nameof(PunchItem)} can not be uncleared");
        }
        ClearedAtUtc = null;
        ClearedBy = null;
        ClearedById = null;
    }

    public void Unverify()
    {
        if (!IsReadyToBeUnverified)
        {
            throw new Exception($"{nameof(PunchItem)} can not be unverified");
        }
        VerifiedAtUtc = null;
        VerifiedBy = null;
        VerifiedById = null;
    }

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedBy = createdBy;
        CreatedById = createdBy.Id;
    }

    public void SetModified(Person modifiedBy)
    {
        ModifiedAtUtc = TimeService.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedById = modifiedBy.Id;
    }

    public void SetRaisedByOrg(LibraryItem raisedByOrg)
    {
        if (raisedByOrg.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(raisedByOrg)} in {raisedByOrg.Plant} to item in {Plant}");
        }
        if (raisedByOrg.Type != LibraryType.COMPLETION_ORGANIZATION)
        {
            throw new ArgumentException($"Can't relate a {raisedByOrg.Type} as {nameof(raisedByOrg)}");
        }

        RaisedByOrg = raisedByOrg;
        RaisedByOrgId = raisedByOrg.Id;
    }

    public void SetClearingByOrg(LibraryItem clearingByOrg)
    {
        if (clearingByOrg.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(clearingByOrg)} in {clearingByOrg.Plant} to item in {Plant}");
        }
        if (clearingByOrg.Type != LibraryType.COMPLETION_ORGANIZATION)
        {
            throw new ArgumentException($"Can't relate a {clearingByOrg.Type} as {nameof(clearingByOrg)}");
        }

        ClearingByOrg = clearingByOrg;
        ClearingByOrgId = clearingByOrg.Id;
    }

    public void SetSorting(LibraryItem sorting)
    {
        if (sorting.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(sorting)} in {sorting.Plant} to item in {Plant}");
        }
        if (sorting.Type != LibraryType.PUNCHLIST_SORTING)
        {
            throw new ArgumentException($"Can't relate a {sorting.Type} as {nameof(sorting)}");
        }

        Sorting = sorting;
        SortingId = sorting.Id;
    }

    public void SetType(LibraryItem type)
    {
        if (type.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(type)} in {type.Plant} to item in {Plant}");
        }
        if (type.Type != LibraryType.PUNCHLIST_TYPE)
        {
            throw new ArgumentException($"Can't relate a {type.Type} as {nameof(type)}");
        }

        Type = type;
        TypeId = type.Id;
    }

    public void SetPriority(LibraryItem priority)
    {
        if (priority.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(priority)} in {priority.Plant} to item in {Plant}");
        }
        if (priority.Type != LibraryType.COMM_PRIORITY)
        {
            throw new ArgumentException($"Can't relate a {priority.Type} as {nameof(priority)}");
        }

        Priority = priority;
        PriorityId = priority.Id;
    }

    public void ClearSorting()
    {
        Sorting = null;
        SortingId = null;
    }

    public void ClearPriority()
    {
        Priority = null;
        PriorityId = null;
    }

    public void ClearType()
    {
        Type = null;
        TypeId = null;
    }

    public void SetWorkOrder(WorkOrder workOrder)
    {
        if (workOrder.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(workOrder)} in {workOrder.Plant} to item in {Plant}");
        }

        WorkOrder = workOrder;
        WorkOrderId = workOrder.Id;
    }

    public void ClearWorkOrder()
    {
        WorkOrder = null;
        WorkOrderId = null;
    }

    public void SetOriginalWorkOrder(WorkOrder workOrder)
    {
        if (workOrder.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(workOrder)} in {workOrder.Plant} to item in {Plant}");
        }

        OriginalWorkOrder = workOrder;
        OriginalWorkOrderId = workOrder.Id;
    }

    public void ClearOriginalWorkOrder()
    {
        OriginalWorkOrder = null;
        OriginalWorkOrderId = null;
    }

    public void SetDocument(Document document)
    {
        if (document.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(document)} in {document.Plant} to item in {Plant}");
        }

        Document = document;
        DocumentId = document.Id;
    }

    public void ClearDocument()
    {
        Document = null;
        DocumentId = null;
    }

    public void SetSWCR(SWCR swcr)
    {
        if (swcr.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(swcr)} in {swcr.Plant} to item in {Plant}");
        }

        SWCR = swcr;
        SWCRId = swcr.Id;
    }

    public void ClearSWCR()
    {
        SWCR = null;
        SWCRId = null;
    }

    public void SetActionBy(Person person)
    {
        ActionBy = person;
        ActionById = person.Id;
    }

    public void ClearActionBy()
    {
        ActionBy = null;
        ActionById = null;
    }

    private void SetProject(string plant, Project project)
    {
        if (project.Plant != plant)
        {
            throw new ArgumentException($"Can't relate {nameof(project)} in {project.Plant} to item in {plant}");
        }

        ProjectId = project.Id;
        Project = project;
    }

    public void SetSyncProperties(
        Person? createdBy, 
        DateTime createdAt,
        Person? modifiedBy,
        DateTime? modifiedAt,
        Person? clearedBy,
        DateTime? clearedAt,
        Person? rejectedBy,
        DateTime? rejectedAt,
        Person? verifiedBy,
        DateTime? verifiedAt,
        Person? actionBy,
        long itemNo
        )
    {
        if(createdBy != null)
        {
            CreatedBy = createdBy;
        }

        CreatedAtUtc = createdAt;
        ModifiedBy = modifiedBy;
        ModifiedAtUtc = modifiedAt;
        ClearedBy = clearedBy;
        ClearedAtUtc = clearedAt;
        RejectedBy = rejectedBy;
        RejectedAtUtc = rejectedAt;
        VerifiedBy = verifiedBy;
        VerifiedAtUtc = verifiedAt;
        ActionBy = actionBy;
        ItemNo = itemNo;
    }

    public List<IProperty> GetRequiredProperties()
        =>
        [
            new Property(nameof(Category), Category.ToString()),
            new Property(nameof(Description), Description),
            new Property(nameof(RaisedByOrg), RaisedByOrg.ToString()),
            new Property(nameof(ClearingByOrg), ClearingByOrg.ToString())
        ];
}
