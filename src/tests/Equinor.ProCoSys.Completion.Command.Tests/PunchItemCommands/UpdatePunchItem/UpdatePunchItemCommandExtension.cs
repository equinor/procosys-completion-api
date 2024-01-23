using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItem;

public static class UpdatePunchItemCommandExtension
{
    // Helper extension for programmer to ensure correct setup of UpdatePunchItemCommand in unit tests
    // This due to rather complex input rules for UpdatePunchItemCommand.PatchDocument
    public static void EnsureValidInputValidation(this UpdatePunchItemCommand command)
    {
        var inputValidator = new PatchPunchItemDtoValidator(new RowVersionInputValidator(), new PatchOperationInputValidator());
        var patchPunchItemDto = new PatchPunchItemDto
        {
            PatchDocument = command.PatchDocument,
            RowVersion = command.RowVersion
        };

        var inputValidationResult = inputValidator.Validate(patchPunchItemDto);
        Assert.IsTrue(inputValidationResult.IsValid);
    }
}
