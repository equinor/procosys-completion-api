using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;

public class Document : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IHaveGuid, IVoidable
{
    public const int NoLengthMax = 60;

#pragma warning disable CS8618
    protected Document()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public Document(string plant, Guid guid, string no)
        : base(plant)
    {
        Guid = guid;
        No = no;
    }

    // private setters needed for Entity Framework
    public string No { get; set; }
    public bool IsVoided { get; set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public Guid Guid { get; private set; }

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
}
