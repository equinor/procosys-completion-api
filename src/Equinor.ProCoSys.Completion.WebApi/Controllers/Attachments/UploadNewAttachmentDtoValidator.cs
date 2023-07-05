using Equinor.ProCoSys.BlobStorage;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;

public class UploadNewAttachmentDtoValidator : UploadBaseDtoValidator<UploadNewAttachmentDto>
{
    public UploadNewAttachmentDtoValidator(IOptionsSnapshot<BlobStorageOptions> blobStorageOptions)
    : base(blobStorageOptions)
    {
    }
}
