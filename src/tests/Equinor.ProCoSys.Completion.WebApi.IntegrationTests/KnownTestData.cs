using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class KnownTestData(string plant)
{
    public string Plant { get; } = plant;
    public PunchItem PunchItemA { get; set; }
    public PunchItem PunchItemB { get; set; }
    public Guid CommentInPunchItemAGuid { get; set; }
    public Guid AttachmentInPunchItemAGuid { get; set; }
    public HistoryItem HistoryInPunchItemA { get; set; }
    public List<Guid> PunchLibraryItemGuids { get; } = new();
}
