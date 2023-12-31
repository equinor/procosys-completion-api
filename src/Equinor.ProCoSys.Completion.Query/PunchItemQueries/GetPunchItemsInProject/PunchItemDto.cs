﻿using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

public record PunchItemDto(
    Guid Guid,
    string ProjectName,
    int ItemNo,
    Category Category,
    string Description,
    string RowVersion);
