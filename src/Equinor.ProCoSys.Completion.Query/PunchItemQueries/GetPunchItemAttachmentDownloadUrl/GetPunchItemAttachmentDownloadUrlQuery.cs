using System;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;

public class GetPunchItemAttachmentDownloadUrlQuery(Guid punchItemGuid, Guid attachmentGuid)
    : INeedProjectAccess, IRequest<Uri>, IIsPunchItemRelatedQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public ProjectDetailsDto ProjectDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => ProjectDetailsDto.Guid;
    public Guid AttachmentGuid { get; } = attachmentGuid;
}
