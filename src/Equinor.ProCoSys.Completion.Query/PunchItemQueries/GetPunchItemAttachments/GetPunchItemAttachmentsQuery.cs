﻿using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.Attachments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;

public class GetPunchItemAttachmentsQuery : IRequest<Result<IEnumerable<AttachmentDto>>>, IIsPunchItemQuery
{
    public GetPunchItemAttachmentsQuery(Guid punchItemGuid, string? fromIpAddress, string? toIpAddress) => 
        (PunchItemGuid, FromIpAddress, ToIpAddress) = (punchItemGuid, fromIpAddress, toIpAddress);

    public Guid PunchItemGuid { get; }
    public string? FromIpAddress { get; }
    public string? ToIpAddress { get; }
}
