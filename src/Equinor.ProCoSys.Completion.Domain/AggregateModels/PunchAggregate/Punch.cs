using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

public class Punch : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IVoidable, IHaveGuid
{
    public const int TitleLengthMin = 3;
    public const int TitleLengthMax = 250;
    public const int TextLengthMax = 500;

#pragma warning disable CS8618
    protected Punch()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public Punch(string plant, Project project, string title)
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

        Title = title;
        Guid = Guid.NewGuid();
    }

    // private setters needed for Entity Framework
    public int ProjectId { get; private set; }
    public string Title { get; set; }
    public string? Text { get; set; }
    public bool IsVoided { get; set; }

    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Guid CreatedByOid { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Guid? ModifiedByOid { get; private set; }
    public Guid Guid { get; private set; }

    public void Update(string title, string? text)
    {
        Title = title;
        Text = text;
    }

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        if (createdBy == null)
        {
            throw new ArgumentNullException(nameof(createdBy));
        }
        CreatedById = createdBy.Id;
        CreatedByOid = createdBy.Guid;
    }

    public void SetModified(Person modifiedBy)
    {
        ModifiedAtUtc = TimeService.UtcNow;
        if (modifiedBy == null)
        {
            throw new ArgumentNullException(nameof(modifiedBy));
        }
        ModifiedById = modifiedBy.Id;
        ModifiedByOid = modifiedBy.Guid;
    }
}
