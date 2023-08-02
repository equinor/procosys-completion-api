using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Command;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

// todo IIsProjectCommand should be IIsProjectQuery. Remove dependency to Command project (check scaffold repo too!)
public class GetPunchItemsInProjectQuery : IRequest<Result<IEnumerable<PunchItemDto>>>, IIsProjectCommand
{
    public GetPunchItemsInProjectQuery(Guid projectGuid) => ProjectGuid = projectGuid;

    public Guid ProjectGuid { get; }
}
