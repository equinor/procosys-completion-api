using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;

public class GetPunchesInProjectQuery : IRequest<Result<IEnumerable<PunchDto>>>, IIsProjectCommand
{
    public GetPunchesInProjectQuery(Guid projectGuid, bool includeVoided = false)
    {
        ProjectGuid = projectGuid;
        IncludeVoided = includeVoided;
    }

    public Guid ProjectGuid { get; }
    public bool IncludeVoided { get; }
}
