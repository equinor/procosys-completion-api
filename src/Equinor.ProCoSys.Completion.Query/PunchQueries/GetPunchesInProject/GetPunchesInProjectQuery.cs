using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;

public class GetPunchesInProjectQuery : IRequest<Result<IEnumerable<PunchDto>>>, IIsProjectCommand
{
    public GetPunchesInProjectQuery(string projectName, bool includeVoided = false)
    {
        ProjectName = projectName;
        IncludeVoided = includeVoided;
    }

    public string ProjectName { get; }
    public bool IncludeVoided { get; }
}
