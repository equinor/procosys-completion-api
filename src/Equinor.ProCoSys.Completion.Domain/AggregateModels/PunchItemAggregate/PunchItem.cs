using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

public class PunchItem : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IHaveGuid
{
    public const int IdentitySeed = 4000001;
    public const int DescriptionLengthMax = 2000;

#pragma warning disable CS8618
    protected PunchItem()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public PunchItem(
        string plant,
        Project project,
        Guid checklistGuid,
        string description,
        LibraryItem raisedByOrg,
        LibraryItem clearingByOrg)
        : base(plant)
    {
        if (project.Plant != plant)
        {
            throw new ArgumentException($"Can't relate {nameof(project)} in {project.Plant} to item in {plant}");
        }
        if (raisedByOrg.Plant != plant)
        {
            throw new ArgumentException($"Can't relate {nameof(raisedByOrg)} in {raisedByOrg.Plant} to item in {plant}");
        }
        if (raisedByOrg.Type != LibraryType.COMPLETION_ORGANIZATION)
        {
            throw new ArgumentException($"Can't relate a {raisedByOrg.Type} as {nameof(raisedByOrg)}");
        }
        if (clearingByOrg.Plant != plant)
        {
            throw new ArgumentException($"Can't relate {nameof(clearingByOrg)} in {clearingByOrg.Plant} to item in {plant}");
        }
        if (clearingByOrg.Type != LibraryType.COMPLETION_ORGANIZATION)
        {
            throw new ArgumentException($"Can't relate a {clearingByOrg.Type} as {nameof(clearingByOrg)}");
        }
        ProjectId = project.Id;
        ChecklistGuid = checklistGuid;
        RaisedByOrgId = raisedByOrg.Id;
        ClearingByOrgId = clearingByOrg.Id;
        Description = description;
        Guid = Guid.NewGuid();
    }

    // private setters needed for Entity Framework
    public int ProjectId { get; private set; }
    // Guid to Checklist in ProCoSys 4 owning the Punch. Will probably be an internal Id to Internal Checklist table when Checklist migrated to Completion
    public Guid ChecklistGuid { get; private set; }
    public int ItemNo => Id;
    public string Description { get; set; }
    public int RaisedByOrgId { get; private set; }
    public int ClearingByOrgId { get; private set; }
    public int? SortingId { get; private set; }
    public int? TypeId { get; private set; }
    public int? PriorityId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Guid CreatedByOid { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Guid? ModifiedByOid { get; private set; }
    public Guid Guid { get; private set; }
    public DateTime? ClearedAtUtc { get; private set; }
    public int? ClearedById { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }
    public int? RejectedById { get; private set; }
    public DateTime? VerifiedAtUtc { get; private set; }
    public int? VerifiedById { get; private set; }

    public bool IsReadyToBeCleared => !ClearedAtUtc.HasValue;
    public bool IsReadyToBeRejected => ClearedAtUtc.HasValue && !VerifiedAtUtc.HasValue;
    public bool IsReadyToBeVerified => ClearedAtUtc.HasValue && !VerifiedAtUtc.HasValue;
    public bool IsReadyToBeUncleared => ClearedAtUtc.HasValue && !VerifiedAtUtc.HasValue;
    public bool IsReadyToBeUnverified => VerifiedAtUtc.HasValue;

    public void Clear(Person clearedBy)
    {
        if (!IsReadyToBeCleared)
        {
            throw new Exception($"{nameof(PunchItem)} can not be cleared");
        }
        ClearedAtUtc = TimeService.UtcNow;
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
        RejectedById = rejectedBy.Id;
        ClearedAtUtc = null;
        ClearedById = null;
    }

    public void Verify(Person verifiedBy)
    {
        if (!IsReadyToBeVerified)
        {
            throw new Exception($"{nameof(PunchItem)} can not be verified");
        }
        VerifiedAtUtc = TimeService.UtcNow;
        VerifiedById = verifiedBy.Id;
    }

    public void Unclear()
    {
        if (!IsReadyToBeUncleared)
        {
            throw new Exception($"{nameof(PunchItem)} can not be uncleared");
        }
        ClearedAtUtc = null;
        ClearedById = null;
    }

    public void Unverify()
    {
        if (!IsReadyToBeUnverified)
        {
            throw new Exception($"{nameof(PunchItem)} can not be unverified");
        }
        VerifiedAtUtc = null;
        VerifiedById = null;
    }

    public void Update(string description) => Description = description;

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedById = createdBy.Id;
        CreatedByOid = createdBy.Guid;
    }

    public void SetModified(Person modifiedBy)
    {
        ModifiedAtUtc = TimeService.UtcNow;
        ModifiedById = modifiedBy.Id;
        ModifiedByOid = modifiedBy.Guid;
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

        TypeId = type.Id;
    }

    public void SetPriority(LibraryItem priority)
    {
        if (priority.Plant != Plant)
        {
            throw new ArgumentException($"Can't relate {nameof(priority)} in {priority.Plant} to item in {Plant}");
        }
        if (priority.Type != LibraryType.PUNCHLIST_PRIORITY)
        {
            throw new ArgumentException($"Can't relate a {priority.Type} as {nameof(priority)}");
        }

        PriorityId = priority.Id;
    }
}
