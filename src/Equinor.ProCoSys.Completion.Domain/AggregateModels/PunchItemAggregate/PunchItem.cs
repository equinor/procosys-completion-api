using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

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

    public PunchItem(string plant, Project project, string description)
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
        Description = description;
        Guid = Guid.NewGuid();
    }

    // private setters needed for Entity Framework
    public int ProjectId { get; private set; }
    public int ItemNo => Id;
    public string Description { get; set; }

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
}
