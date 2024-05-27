using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;

public class Document : PlantEntityBase, IAggregateRoot, IHaveGuid, IVoidable
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
    public Guid Guid { get; private set; }
    public DateTime ProCoSys4LastUpdated { get;  set; }
    public DateTime SyncTimestamp { get; set; }
}
