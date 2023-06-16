using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Attachments;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachmentDownloadUrl;

public class GetPunchAttachmentDownloadUrlQueryHandler : IRequestHandler<GetPunchAttachmentDownloadUrlQuery, Result<Uri>>
{
    private readonly IAttachmentService _attachmentService;

    public GetPunchAttachmentDownloadUrlQueryHandler(IAttachmentService attachmentService) => _attachmentService = attachmentService;

    public async Task<Result<Uri>> Handle(GetPunchAttachmentDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var uri = await _attachmentService.TryGetDownloadUriAsync(request.AttachmentGuid, cancellationToken);
        if (uri == null)
        {
            return new NotFoundResult<Uri>($"Attachment with Guid {request.AttachmentGuid} not found");
        }

        return new SuccessResult<Uri>(uri);
    }
}
