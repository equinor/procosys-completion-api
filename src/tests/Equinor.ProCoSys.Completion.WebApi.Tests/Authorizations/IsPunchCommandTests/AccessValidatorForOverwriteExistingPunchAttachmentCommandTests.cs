using Equinor.ProCoSys.Completion.Command.PunchCommands.OverwriteExistingPunchAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForOverwriteExistingPunchAttachmentCommandTests : AccessValidatorForIIsPunchCommandTests<OverwriteExistingPunchAttachmentCommand>
{
    protected override OverwriteExistingPunchAttachmentCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, null!, null!, null!);

    protected override OverwriteExistingPunchAttachmentCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, null!, null!, null!);
}
