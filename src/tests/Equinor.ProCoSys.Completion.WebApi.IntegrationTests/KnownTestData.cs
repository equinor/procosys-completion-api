using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class KnownTestData
{
    public KnownTestData(string plant)
    {
        Plant = plant;
        PunchLibraryItemGuids = new List<Guid>();
    }

    public string Plant { get; }
    public PunchItem PunchItem { get; set; }
    public Guid LinkInPunchItemAGuid { get; set; }
    public Guid CommentInPunchItemAGuid { get; set; }
    public Guid AttachmentInPunchItemAGuid { get; set; }
    public HistoryItem HistoryInPunchItem { get; set; }


    public List<Guid> PunchLibraryItemGuids { get; }
}
