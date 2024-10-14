using System.IO;
using System.Linq;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;

public class UploadBaseDtoValidator<T> : AbstractValidator<T> where T : UploadBaseDto
{
    public UploadBaseDtoValidator(IOptionsSnapshot<BlobStorageOptions> blobStorageOptions)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .NotNull();

        RuleFor(x => x.File)
            .NotNull();

        RuleFor(x => x.File.FileName)
            .NotEmpty()
            .WithMessage("File name not given!")
            .MaximumLength(Attachment.FileNameLengthMax)
            .WithMessage($"File name to long! Max {Attachment.FileNameLengthMax} characters")
            .Must(BeValidFileType)
            .WithMessage(x => $"File {x.File.FileName} is not a valid file type!");

        RuleFor(x => x.File.Length)
            .Must(BeSmallerThanMaxSize)
            .WithMessage($"Maximum file size is {blobStorageOptions.Value.MaxSizeMb}MB!");

        bool BeValidFileType(string? fileName)
        {
            var suffix = Path.GetExtension(fileName?.ToLower());
            return suffix is not null && !blobStorageOptions.Value.BlockedFileSuffixes.Contains(suffix) && fileName?.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
        }

        bool BeSmallerThanMaxSize(long fileSizeInBytes)
        {
            var maxSizeInBytes = blobStorageOptions.Value.MaxSizeMb * 1024 * 1024;
            return fileSizeInBytes < maxSizeInBytes;
        }
    }
}
