using System.IO;
using System.Linq;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

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
            .WithMessage("Filename not given!")
            .MaximumLength(Attachment.FileNameLengthMax)
            .WithMessage($"Filename to long! Max {Attachment.FileNameLengthMax} characters")
            .Must(BeAValidFile)
            .WithMessage(x => $"File {x.File.FileName} is not a valid file for upload!");
            
        RuleFor(x => x.File.Length)
            .Must(BeSmallerThanMaxSize)
            .WithMessage($"Maximum file size is {blobStorageOptions.Value.MaxSizeMb}MB!");

        bool BeAValidFile(string? fileName)
        {
            var suffix = Path.GetExtension(fileName?.ToLower());
            return suffix != null && !blobStorageOptions.Value.BlockedFileSuffixes.Contains(suffix) && fileName?.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
        }
            
        bool BeSmallerThanMaxSize(long fileSizeInBytes)
        {
            var maxSizeInBytes = blobStorageOptions.Value.MaxSizeMb * 1024 * 1024;
            return fileSizeInBytes < maxSizeInBytes;
        }
    }
}
