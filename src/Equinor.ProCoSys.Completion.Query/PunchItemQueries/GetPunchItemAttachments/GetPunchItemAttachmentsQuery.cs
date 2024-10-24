﻿using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;

public class GetPunchItemAttachmentsQuery(Guid punchItemGuid, string? fromIpAddress, string? toIpAddress)
    : INeedProjectAccess, IRequest<IEnumerable<AttachmentDto>>, IIsPunchItemRelatedQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public ProjectDetailsDto ProjectDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => ProjectDetailsDto.Guid;
    public string? FromIpAddress { get; } = fromIpAddress;
    public string? ToIpAddress { get; } = toIpAddress;
}
