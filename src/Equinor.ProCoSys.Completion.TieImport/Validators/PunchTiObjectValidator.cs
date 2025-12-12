using Equinor.ProCoSys.Completion.TieImport.Extensions;
using FluentValidation;
using Statoil.TI.InterfaceServices.Message;
using System.Globalization;
using static Equinor.ProCoSys.Completion.TieImport.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Validators;

public sealed class PunchTiObjectValidator : AbstractValidator<TIObject>
{
    private const string ExpectedDateFormat = "yyyy-MM-dd";
    
    public PunchTiObjectValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Continue;
        ClassLevelCascadeMode = CascadeMode.Continue;

        // Method validation
        RuleFor(tiObject => tiObject.Method)
            .Must(method => method == Methods.Create || method == Methods.Update || method == Methods.Append)
            .WithMessage($"Method must be either '{Methods.Create}', '{Methods.Update}' or '{Methods.Append}'");

        // Project validation
        RuleFor(tiObject => tiObject.Project)
            .NotEmpty()
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{Project}'");
        
        // CREATE-specific validation: Status transition fields should not be set, Description required
        When(tiObject => tiObject.Method is Methods.Create, () =>
        {
            RuleFor(o => o)
                .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(Description)))
                .WithMessage($"'{Description}' is required for CREATE")
                .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(RaisedByOrganization)))
                .WithMessage($"'{RaisedByOrganization}' is required for CREATE")
                .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(ClearedByOrganization)))
                .WithMessage($"'{ClearedByOrganization}' is required for CREATE")
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

        // Required attributes validation
        RuleFor(tiObject => tiObject)
            .Must(tiObject => tiObject.GetAttributeValueAsString(Status) is "PA" or "PB")
            .WithMessage($"This Punch Item Import Object has illegal required attribute '{Status}'. Must be PA or PB")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(TagNo)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{TagNo}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(ExternalPunchItemNo)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{ExternalPunchItemNo}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(FormType)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{FormType}'")
            .Must(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(Responsible)))
            .WithMessage($"This Punch Item Import Object is missing the required attribute '{Responsible}'")
            .Must(tiObject => IsValidLong(tiObject.GetAttributeValueAsString(PunchItemNo)))
            .WithMessage($"'{PunchItemNo}' must be a valid number");

        // Material fields validation
        RuleFor(tiObject => tiObject)
            .Must(tiObject => IsValidYesNo(tiObject.GetAttributeValueAsString(MaterialRequired)))
            .WithMessage($"'{MaterialRequired}' must be 'Y' or 'N'")
            .Must(tiObject => IsValidOptionalDateFormat(tiObject.GetAttributeValueAsString(MaterialEta)))
            .WithMessage($"'{MaterialEta}' must be in format '{ExpectedDateFormat}'");

        // Cleared fields validation
        AddActionByDatePairValidation(ClearedBy, ClearedDate);
        
        // Verified fields validation
        AddActionByDatePairValidation(VerifiedBy, VerifiedDate);
        
        // Rejected fields validation
        AddActionByDatePairValidation(RejectedBy, RejectedDate);

        // DueDate validation
        RuleFor(tiObject => tiObject)
            .Must(tiObject => IsValidOptionalDateFormat(tiObject.GetAttributeValueAsString(DueDate)))
            .WithMessage($"'{DueDate}' must be in format '{ExpectedDateFormat}'");
    }

    private void AddActionByDatePairValidation(string actionByAttribute, string dateAttribute)
    {
        RuleFor(tiObject => tiObject)
            .Must(tiObject => IsActionByPresentWhenDateProvided(tiObject, actionByAttribute, dateAttribute))
            .WithMessage($"'{actionByAttribute}' is required when '{dateAttribute}' is provided")
            .Must(tiObject => IsDatePresentWhenActionByProvided(tiObject, actionByAttribute, dateAttribute))
            .WithMessage($"'{dateAttribute}' is required when '{actionByAttribute}' is provided")
            .Must(tiObject => IsValidRequiredDateFormat(tiObject.GetAttributeValueAsString(dateAttribute)))
            .WithMessage($"'{dateAttribute}' must be in format '{ExpectedDateFormat}'")
            .When(tiObject => !string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(actionByAttribute)), ApplyConditionTo.CurrentValidator);
    }

    private static bool IsDatePresentWhenActionByProvided(TIObject tiObject, string actionByAttribute, string dateAttribute)
    {
        var actionByValue = tiObject.GetAttributeValueAsString(actionByAttribute);
        var dateValue = tiObject.GetAttributeValueAsString(dateAttribute);
        return string.IsNullOrEmpty(actionByValue) || !string.IsNullOrEmpty(dateValue);
    }

    private static bool IsActionByPresentWhenDateProvided(TIObject tiObject, string actionByAttribute, string dateAttribute)
    {
        var actionByValue = tiObject.GetAttributeValueAsString(actionByAttribute);
        var dateValue = tiObject.GetAttributeValueAsString(dateAttribute);
        return string.IsNullOrEmpty(dateValue) || !string.IsNullOrEmpty(actionByValue);
    }

    private static bool IsValidOptionalDateFormat(string? dateValue)
    {
        if (string.IsNullOrEmpty(dateValue) || dateValue == NullMarker)
        {
            return true;
        }
        return DateTime.TryParseExact(dateValue, ExpectedDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out _);
    }

    private static bool IsValidRequiredDateFormat(string? dateValue)
    {
        if (string.IsNullOrEmpty(dateValue))
        {
            return true;
        }
        return DateTime.TryParseExact(dateValue, ExpectedDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out _);
    }

    private static bool IsValidYesNo(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }
        return value.Equals("Y", StringComparison.OrdinalIgnoreCase) || 
               value.Equals("N", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidLong(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }
        return long.TryParse(value.Trim(), out _);
    }
}
