using System;
using System.IO;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

public class Attachment : EntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IBelongToParent, IHaveGuid
{
    public const int ParentTypeLengthMax = 256;
    public const int FileNameLengthMax = 255;
    public const int BlobPathLengthMax = 1024;

#pragma warning disable CS8618
    public Attachment()
#pragma warning restore CS8618
    {
    }

    public Attachment(string parentType, Guid parentGuid, string plant, string fileName)
    {
        ParentType = parentType;
        ParentGuid = parentGuid;
        FileName = fileName;
        Guid = MassTransit.NewId.NextGuid();
        if (plant.Length < 5)
        {
            throw new ArgumentException($"{nameof(plant)} must have minimum length 5");
        }
        BlobPath = Path.Combine(plant.Substring(4), ParentType, Guid.ToString()).Replace("\\", "/");
        RevisionNumber = 1;
    }

    // private setters needed for Entity Framework
    public string ParentType { get; private set; }
    public Guid ParentGuid { get; private set; }
    public string FileName { get; private set; }
    public string BlobPath { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public Guid Guid { get; private set; }
    public int RevisionNumber { get; private set; }

    public void IncreaseRevisionNumber() => RevisionNumber++;

    public string GetFullBlobPath()
        => Path.Combine(BlobPath, FileName).Replace("\\", "/");

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
