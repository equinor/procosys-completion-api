using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;

public class SWCR : PlantEntityBase, IAggregateRoot, IHaveGuid, IVoidable
{
    protected SWCR()
        : base(null)
    {
    }

    public SWCR(string plant, Guid guid, int no)
        : base(plant)
    {
        Guid = guid;
        No = no;
    }

    // private setters needed for Entity Framework
    public int No { get; set; }
    public bool IsVoided { get; set; }
    public Guid Guid { get; private set; }
    public DateTime ProCoSys4LastUpdated { get; set; }
    public DateTime SyncedTimeStamp { get; set; }
}
