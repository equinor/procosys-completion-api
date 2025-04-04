﻿using Equinor.ProCoSys.Completion.TieImport.Models;
using FluentValidation;
using static Equinor.ProCoSys.Completion.TieImport.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Validators;

public sealed class PunchItemImportMessageValidator : AbstractValidator<PunchItemImportMessage>
{
    public PunchItemImportMessageValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Continue;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(message => message.Category)
            .Must(category => category is not null)
            .WithMessage($"Punch Item {nameof(PunchItemImportMessage.Category)} is required");

        RuleFor(message => message.Description)
            .Must(description => description.HasValue)
            .When(message => message.Method == Methods.Create)
            .WithMessage("Punch Item Description is required for Create method.");

        RuleFor(message => message.RejectedDate)
            .Must(rejectedDate => rejectedDate is { HasValue: true, Value: not null })
            .When(message => message.RejectedBy is { HasValue: true, Value: not null })
            .WithMessage("Need to set both RejectedDate and RejectedBy");

        RuleFor(message => message.RejectedBy)
            .Must(rejectedBy => rejectedBy is { HasValue: true, Value: not null })
            .When(message => message.RejectedDate is { HasValue: true, Value: not null })
            .WithMessage("Need to set both RejectedDate and RejectedBy");

        RuleFor(message => message.VerifiedDate)
            .Must(verifiedDate => verifiedDate is { HasValue: true, Value: not null })
            .When(message => message.VerifiedBy is { HasValue: true, Value: not null })
            .WithMessage("Need to set both VerifiedDate and VerifiedBy");

        RuleFor(message => message.VerifiedBy)
            .Must(verifiedBy => verifiedBy is { HasValue: true, Value: not null })
            .When(message => message.VerifiedDate is { HasValue: true, Value: not null })
            .WithMessage("Need to set both VerifiedDate and VerifiedBy");

        RuleFor(message => message.ClearedDate)
            .Must(clearedDate => clearedDate is { HasValue: true, Value: not null })
            .When(message => message.ClearedBy is { HasValue: true, Value: not null })
            .WithMessage("Need to set both ClearedDate and ClearedBy");

        RuleFor(message => message.ClearedBy)
            .Must(clearedBy => clearedBy is { HasValue: true, Value: not null })
            .When(message => message.ClearedDate is { HasValue: true, Value: not null })
            .WithMessage("Need to set both ClearedDate and ClearedBy");
    }
}
