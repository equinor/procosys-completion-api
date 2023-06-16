using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Attachments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachments;

public class GetPunchAttachmentsQueryHandler : IRequestHandler<GetPunchAttachmentsQuery, Result<IEnumerable<AttachmentDto>>>
{
    private readonly IAttachmentService _attachmentService;

    public GetPunchAttachmentsQueryHandler(IAttachmentService attachmentService) => _attachmentService = attachmentService;

    public async Task<Result<IEnumerable<AttachmentDto>>> Handle(GetPunchAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var attachmentDtos = await _attachmentService.GetAllForSourceAsync(request.PunchGuid, cancellationToken);
        return new SuccessResult<IEnumerable<AttachmentDto>>(attachmentDtos);
    }
}
