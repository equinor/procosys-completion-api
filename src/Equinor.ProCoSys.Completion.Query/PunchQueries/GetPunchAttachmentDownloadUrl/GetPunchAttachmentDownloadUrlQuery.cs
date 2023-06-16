using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachmentDownloadUrl;

public class GetPunchAttachmentDownloadUrlQuery : IRequest<Result<Uri>>, IIsPunchQuery
{
    public GetPunchAttachmentDownloadUrlQuery(Guid punchGuid, Guid attachmentGuid)
    {
        PunchGuid = punchGuid;
        AttachmentGuid = attachmentGuid;
    }

    public Guid PunchGuid { get; }
    public Guid AttachmentGuid { get; }
}
