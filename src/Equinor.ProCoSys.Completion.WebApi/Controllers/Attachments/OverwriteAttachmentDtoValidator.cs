using FluentValidation;
using Equinor.ProCoSys.BlobStorage;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;

public class OverwriteAttachmentDtoValidator : UploadBaseDtoValidator<OverwriteAttachmentDto>
{
    public OverwriteAttachmentDtoValidator(IRowVersionInputValidator rowVersionValidator,
        IOptionsSnapshot<BlobStorageOptions> blobStorageOptions)
        : base(blobStorageOptions)
    {
        RuleFor(dto => dto.RowVersion)
            .NotNull()
            .Must(HaveValidRowVersion)
            .WithMessage(dto => $"Dto does not have valid rowVersion! RowVersion={dto.RowVersion}");

        bool HaveValidRowVersion(string rowVersion)
            => rowVersionValidator.IsValid(rowVersion);
    }
}
