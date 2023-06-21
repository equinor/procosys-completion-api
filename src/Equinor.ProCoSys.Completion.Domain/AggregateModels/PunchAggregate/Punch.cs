using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

public class Punch : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IVoidable, IHaveGuid
{
    public const int ItemNoLengthMin = 3;
    public const int ItemNoLengthMax = 64;
    public const int DescriptionLengthMax = 2000;

#pragma warning disable CS8618
    protected Punch()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public Punch(string plant, Project project, string itemNo)
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
    public bool IsVoided { get; set; } // todo remove, punch is not voidable

    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Guid CreatedByOid { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Guid? ModifiedByOid { get; private set; }
    public Guid Guid { get; private set; }

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
