using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

public class WorkOrder : PlantEntityBase, IAggregateRoot, IHaveGuid
{
    public const int NoLengthMax = 30;

#pragma warning disable CS8618
    protected WorkOrder()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public WorkOrder(string plant, Guid guid, string no)
        : base(plant)
    {
        Guid = guid;
        No = no;
    }

    // private setters needed for Entity Framework
    public string No { get; set; }
    public bool IsVoided { get; set; }
    public Guid Guid { get; private set; }
    public DateTime ProCoSys4LastUpdated { get; set; }
    public DateTime SyncedTimeStamp { get; set; }
    
}
