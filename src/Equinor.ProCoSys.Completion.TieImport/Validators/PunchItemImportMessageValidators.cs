using Equinor.ProCoSys.Completion.Domain.Imports;
using FluentValidation;
using static Equinor.ProCoSys.Completion.Domain.Imports.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Validators;

public sealed class PunchItemImportMessageValidator : AbstractValidator<PunchItemImportMessage>
{
    public PunchItemImportMessageValidator(PlantScopedImportDataContext scopedImportDataContext)
    {
        RuleLevelCascadeMode = CascadeMode.Continue;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(message => message)
            .Must(x => x.Category is not null)
            .WithMessage($"Punch Item {nameof(PunchItemImportMessage.Category)} is required")
            .Must(x => x.Description.HasValue)
            .When(x => x.Method == Methods.Create)
            .WithMessage("Punch Item Description is required for Create method.")
            .Must(x => x.RejectedDate is { HasValue: true, Value: not null } && x.RejectedBy is {HasValue: true, Value: not null})
            .When(x => x.RejectedDate is { HasValue: true, Value: not null } || x.RejectedBy is {HasValue: true, Value: not null})
            .WithMessage("Need to set both RejectedDate and RejectedBy")
            .Must(x => x.VerifiedDate is { HasValue: true, Value: not null } && x.VerifiedBy is {HasValue: true, Value: not null})
            .When(x => x.VerifiedDate is { HasValue: true, Value: not null } || x.VerifiedBy is {HasValue: true, Value: not null})
            .WithMessage("Need to set both VerifiedDate and VerifiedBy")
            .Must(x => x.ClearedDate is { HasValue: true, Value: not null } && x.ClearedBy is {HasValue: true, Value: not null})
            .When(x => x.ClearedDate is { HasValue: true, Value: not null } || x.ClearedBy is {HasValue: true, Value: not null})
            .WithMessage("Need to set both ClearedDate and ClearedBy");
    }
}
