using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

public class PunchItem : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IHaveGuid
{
    public const int ItemNoLengthMin = 3;
    public const int ItemNoLengthMax = 64;
    public const int DescriptionLengthMax = 2000;

#pragma warning disable CS8618
    protected PunchItem()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public PunchItem(string plant, Project project, string itemNo)
        : base(plant)
    {
        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (project.Plant != plant)
        {
            throw new ArgumentException($"Can't relate item in {project.Plant} to item in {plant}");
        }
        ProjectId = project.Id;

        ItemNo = itemNo;
        Guid = Guid.NewGuid();
    }

    // private setters needed for Entity Framework
    public int ProjectId { get; private set; }
    // todo #104033 How should we generate ItemNo? End user should not need to add it
    public string ItemNo { get; private set; }
    public string? Description { get; set; }

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

    public void Clear(Person clearedBy)
    {
        ClearedAtUtc = TimeService.UtcNow;
        ClearedById = clearedBy.Id;
        RejectedAtUtc = null;
        RejectedById = null;
    }

    public void Reject(Person rejectedBy)
    {
        RejectedAtUtc = TimeService.UtcNow;
        RejectedById = rejectedBy.Id;
        ClearedAtUtc = null;
        ClearedById = null;
    }

    public void Verify(Person verifiedBy)
    {
        VerifiedAtUtc = TimeService.UtcNow;
        VerifiedById = verifiedBy.Id;
    }

    public void Update(string? description) => Description = description;

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
}
