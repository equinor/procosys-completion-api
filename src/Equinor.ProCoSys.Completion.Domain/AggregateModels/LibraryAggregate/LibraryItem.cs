using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

public class LibraryItem : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IHaveGuid, IVoidable
{
    public const int CodeLengthMax = 255;
    public const int DescriptionLengthMax = 255;
    public const int TypeLengthMax = 30;

#pragma warning disable CS8618
    protected LibraryItem()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public LibraryItem(string plant, Guid guid, string code, string description, LibraryType type)
        : base(plant)
    {
        Guid = guid;
        Code = code;
        Description = description;
        Type = type;
    }

    // private setters needed for Entity Framework
    public Guid Guid { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public LibraryType Type { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public bool IsVoided { get; set; }

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedById = createdBy.Id;
        CreatedBy = createdBy;
    }

    public void SetModified(Person modifiedBy)
    {
        ModifiedAtUtc = TimeService.UtcNow;
        ModifiedById = modifiedBy.Id;
        ModifiedBy = modifiedBy;
    }
}
