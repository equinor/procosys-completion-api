using Equinor.ProCoSys.Completion.TieImport.Extensions;
using FluentValidation;
using Statoil.TI.InterfaceServices.Message;
using static Equinor.ProCoSys.Completion.TieImport.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Validators;

public sealed class PunchTiObjectValidator : AbstractValidator<TIObject>
{
    public PunchTiObjectValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Continue;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(tiObject => tiObject.Project)
            .Must(project => !string.IsNullOrEmpty(project))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{Project}'");
        
        When(tiObject => tiObject.Method is "CREATE" or "INSERT" or "ALLOCATE", () =>
            {
                RuleFor(o => o)
                    .Must(tiObject => string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(ClearedBy)))
                    .WithMessage($"'{ClearedBy}' should not be set for CREATE")
                    .Must(tiObject => string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(ClearedDate)))
                    .WithMessage($"'{ClearedDate}' should not be set for CREATE")
                    .Must(tiObject => string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(RejectedBy)))
                    .WithMessage($"'{RejectedBy}' should not be set for CREATE")
                    .Must(tiObject => string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(RejectedDate)))
                    .WithMessage($"'{RejectedDate}' should not be set for CREATE")
                    .Must(tiObject => string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(VerifiedBy)))
                    .WithMessage($"'{VerifiedBy}' should not be set for CREATE")
                    .Must(tiObject => string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(VerifiedDate)))
                    .WithMessage($"'{VerifiedDate}' should not be set for CREATE");
            });
        
        RuleFor(tiObject => tiObject)
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(TagNo)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{TagNo}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(ExternalPunchItemNo)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{ExternalPunchItemNo}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(FormType)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{FormType}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(Responsible)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{Responsible}'");
    }
}
