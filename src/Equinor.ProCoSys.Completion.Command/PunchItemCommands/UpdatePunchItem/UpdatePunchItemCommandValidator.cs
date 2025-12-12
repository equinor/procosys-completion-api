using Equinor.ProCoSys.Completion.Domain.Validators;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

/// <summary>
/// Validator for UpdatePunchItemCommand.
/// Inherits all validation rules from PatchPunchItemCommandValidator.
/// </summary>
public class UpdatePunchItemCommandValidator : PatchPunchItemCommandValidator<UpdatePunchItemCommand>
{
    public UpdatePunchItemCommandValidator(
        ILibraryItemValidator libraryItemValidator,
        IWorkOrderValidator workOrderValidator,
        ISWCRValidator swcrValidator,
        IDocumentValidator documentValidator)
        : base(libraryItemValidator, workOrderValidator, swcrValidator, documentValidator)
    {
    }
}
