using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries.GetDuplicateInfo;

public class GetDuplicateInfoQuery(Guid checkListGuid)
    : IIsCheckListQuery, IRequest<Result<DuplicateInfoDto>>
{
    public Guid CheckListGuid { get; } = checkListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => CheckListDetailsDto.ProjectGuid;
}
