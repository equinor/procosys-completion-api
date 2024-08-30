using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.GetPunchItems;

public record PunchItemDto(
    Guid Guid,
    string ProjectName,
    long ItemNo,
    Category Category,
    string Description,
    string RowVersion);
