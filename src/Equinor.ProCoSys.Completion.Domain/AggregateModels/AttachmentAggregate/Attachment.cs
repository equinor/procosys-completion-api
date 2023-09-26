using System;
using System.IO;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

public class Attachment : EntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IBelongToSource, IHaveGuid
{
    public const int SourceTypeLengthMax = 256;
    public const int FileNameLengthMax = 255;
    public const int BlobPathLengthMax = 1024;

#pragma warning disable CS8618
    public Attachment()
#pragma warning restore CS8618
    {
    }

    public Attachment(string sourceType, Guid sourceGuid, string plant, string fileName)
    {
        SourceType = sourceType;
        SourceGuid = sourceGuid;
        FileName = fileName;
        Guid = Guid.NewGuid();
        if (plant.Length < 5)
        {
            throw new ArgumentException($"{nameof(plant)} must have minimum length 5");
        }
        BlobPath = Path.Combine(plant.Substring(4), SourceType, Guid.ToString()).Replace("\\", "/");
        RevisionNumber = 1;
    }

    // private setters needed for Entity Framework
    public string SourceType { get; private set; }
    public Guid SourceGuid { get; private set; }
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
