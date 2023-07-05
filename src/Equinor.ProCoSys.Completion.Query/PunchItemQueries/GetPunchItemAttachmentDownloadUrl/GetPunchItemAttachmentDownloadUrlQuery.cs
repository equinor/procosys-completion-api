using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;

public class GetPunchItemAttachmentDownloadUrlQuery : IRequest<Result<Uri>>, IIsPunchItemQuery
{
    public GetPunchItemAttachmentDownloadUrlQuery(Guid punchItemGuid, Guid attachmentGuid)
    {
        PunchItemGuid = punchItemGuid;
        AttachmentGuid = attachmentGuid;
    }

    public Guid PunchItemGuid { get; }
    public Guid AttachmentGuid { get; }
}
