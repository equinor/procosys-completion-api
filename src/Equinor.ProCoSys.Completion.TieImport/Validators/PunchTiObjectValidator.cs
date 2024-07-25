using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Models;
using FluentValidation;
using Statoil.TI.InterfaceServices.Message;
using static Equinor.ProCoSys.Completion.Domain.Imports.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Validators;

public sealed class PunchTiObjectValidator : AbstractValidator<TIObject>
{
    public PunchTiObjectValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Continue;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(tiObject => tiObject)
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.Project))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{Project}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(TagNo)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{TagNo}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(ExternalPunchItemNo)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{ExternalPunchItemNo}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(FormType)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{FormType}'");
    }
}
