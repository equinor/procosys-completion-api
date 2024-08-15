using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.Attachments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;

public class GetPunchItemAttachmentDownloadUrlQueryHandler : IRequestHandler<GetPunchItemAttachmentDownloadUrlQuery, Result<Uri>>
{
    private readonly IAttachmentService _attachmentService;

    public GetPunchItemAttachmentDownloadUrlQueryHandler(IAttachmentService attachmentService) => _attachmentService = attachmentService;

    public async Task<Result<Uri>> Handle(GetPunchItemAttachmentDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var uri = await _attachmentService.GetDownloadUriAsync(request.AttachmentGuid, cancellationToken);
        if (uri is null)
        {
            throw new EntityNotFoundException($"Attachment with Guid {request.AttachmentGuid} not found");
        }

        return new SuccessResult<Uri>(uri);
    }
}
