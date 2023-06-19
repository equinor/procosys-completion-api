using System;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

public class Comment : EntityBase, IAggregateRoot, ICreationAuditable, IBelongToSource, IHaveGuid
{
    public const int SourceTypeLengthMax = 256;
    public const int TextLengthMax = 4000;

    public Comment(string sourceType, Guid sourceGuid, string text)
    {
        SourceType = sourceType;
        SourceGuid = sourceGuid;
        Text = text;
        Guid = Guid.NewGuid();
    }

    // private setters needed for Entity Framework
    public string SourceType { get; private set; }
    public string Text { get; private set; }
    public Guid SourceGuid { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Guid CreatedByOid { get; }
    public Guid Guid { get; private set; }

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        if (createdBy == null)
        {
            throw new ArgumentNullException(nameof(createdBy));
        }
        CreatedById = createdBy.Id;
    }
}
