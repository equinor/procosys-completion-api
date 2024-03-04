using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Attachments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;

public class GetPunchItemAttachmentsQueryHandler : IRequestHandler<GetPunchItemAttachmentsQuery, Result<IEnumerable<AttachmentDto>>>
{
    private readonly IAttachmentService _attachmentService;

    public GetPunchItemAttachmentsQueryHandler(IAttachmentService attachmentService) => _attachmentService = attachmentService;

    public async Task<Result<IEnumerable<AttachmentDto>>> Handle(GetPunchItemAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var attachmentDtos = await _attachmentService.GetAllForParentAsync(request.PunchItemGuid, cancellationToken, request.FromIpAddress, request.ToIpAddress);
        return new SuccessResult<IEnumerable<AttachmentDto>>(attachmentDtos);
    }
}
