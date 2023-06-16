using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.Attachments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachments;

public class GetPunchAttachmentsQuery : IRequest<Result<IEnumerable<AttachmentDto>>>, IIsPunchQuery
{
    public GetPunchAttachmentsQuery(Guid punchGuid) => PunchGuid = punchGuid;

    public Guid PunchGuid { get; }
}
