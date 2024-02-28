using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.Attachments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;

public class GetPunchItemAttachmentsQuery : IRequest<Result<IEnumerable<AttachmentDto>>>, IIsPunchItemQuery
{
    public GetPunchItemAttachmentsQuery(Guid punchItemGuid) => PunchItemGuid = punchItemGuid;
    public GetPunchItemAttachmentsQuery(Guid punchItemGuid, string? fromIPAddress, string? toIPAddress) => (PunchItemGuid, FromIPAddress, ToIPAddress) = (punchItemGuid, fromIPAddress, toIPAddress);

    public Guid PunchItemGuid { get; }
    public string? FromIPAddress { get; }
    public string? ToIPAddress { get; }
}
