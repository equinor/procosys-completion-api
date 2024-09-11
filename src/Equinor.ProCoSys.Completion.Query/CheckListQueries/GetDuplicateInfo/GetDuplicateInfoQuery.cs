using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries.GetDuplicateInfo;

public class GetDuplicateInfoQuery(Guid checkListGuid)
    : IIsCheckListQuery, IRequest<DuplicateInfoDto>
{
    public Guid CheckListGuid { get; } = checkListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => CheckListDetailsDto.ProjectGuid;
}
