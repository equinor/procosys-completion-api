using System;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;

public class GetPunchItemAttachmentDownloadUrlQuery(Guid punchItemGuid, Guid attachmentGuid)
    : NeedProjectAccess, IRequest<Result<Uri>>, IIsPunchItemRelatedQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public ProjectDetailsDto ProjectDetailsDto { get; set; } = null!;
    public override Guid GetProjectGuidForAccessCheck() => ProjectDetailsDto.Guid;
    public Guid AttachmentGuid { get; } = attachmentGuid;
}
