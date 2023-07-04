using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchItemsInProject;

public class GetPunchItemsInProjectQuery : IRequest<Result<IEnumerable<PunchDto>>>, IIsProjectCommand
{
    public GetPunchItemsInProjectQuery(Guid projectGuid) => ProjectGuid = projectGuid;

    public Guid ProjectGuid { get; }
}
