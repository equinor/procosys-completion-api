using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

public class Link : EntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IBelongToParent, IHaveGuid
{
    public const int ParentTypeLengthMax = 256;
    public const int TitleLengthMax = 256;
    public const int UrlLengthMax = 2000;

#pragma warning disable CS8618
    public Link()
#pragma warning restore CS8618
    {
    }

    public Link(string parentType, Guid parentGuid, string title, string url, Guid? proCoSysGuid = null)
    {
        ParentType = parentType;
        ParentGuid = parentGuid;
        Title = title;
        Url = url;
        Guid = proCoSysGuid ?? MassTransit.NewId.NextGuid();
    }

    // private setters needed for Entity Framework
    public string ParentType { get; private set; }
    public Guid ParentGuid { get; private set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public Guid Guid { get; private set; }
    public DateTime ProCoSys4LastUpdated { get; set; }
    public DateTime SyncTimestamp { get; set; }

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

    public void SetSyncProperties(DateTime modifiedAt) => ModifiedAtUtc = modifiedAt;

    public void SetSyncProperties(Person createdBy, DateTime createdAt, DateTime modifiedAt)
    {
        CreatedAtUtc = createdAt;
        CreatedById = createdBy.Id;
        CreatedBy = createdBy;
        SetSyncProperties(modifiedAt);
    }
}
