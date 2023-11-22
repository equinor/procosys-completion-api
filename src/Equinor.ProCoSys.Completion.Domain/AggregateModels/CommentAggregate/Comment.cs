using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

public class Comment : EntityBase, IAggregateRoot, ICreationAuditable, IBelongToParent, IHaveGuid
{
    public const int ParentTypeLengthMax = 256;
    public const int TextLengthMax = 4000;

    public Comment(string parentType, Guid parentGuid, string text)
    {
        ParentType = parentType;
        ParentGuid = parentGuid;
        Text = text;
        Guid = MassTransit.NewId.NextGuid();
    }

    // private setters needed for Entity Framework
    public string ParentType { get; private set; }
    public string Text { get; private set; }
    public Guid ParentGuid { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public Guid Guid { get; private set; }

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedById = createdBy.Id;
        CreatedBy = createdBy;
    }
}
