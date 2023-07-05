using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

public class Link : EntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IBelongToSource, IHaveGuid
{
    public const int SourceTypeLengthMax = 256;
    public const int TitleLengthMax = 256;
    public const int UrlLengthMax = 2000;

    public Link(string sourceType, Guid sourceGuid, string title, string url)
    {
        SourceType = sourceType;
        SourceGuid = sourceGuid;
        Title = title;
        Url = url;
        Guid = Guid.NewGuid();
    }

    // private setters needed for Entity Framework
    public string SourceType { get; private set; }
    public Guid SourceGuid { get; private set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Guid CreatedByOid { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Guid? ModifiedByOid { get; private set; }
    public Guid Guid { get; private set; }

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
