using Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForOverwriteExistingPunchItemAttachmentCommandTests : AccessValidatorForIIsPunchCommandTests<OverwriteExistingPunchItemAttachmentCommand>
{
    protected override OverwriteExistingPunchItemAttachmentCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!, null!, null!);

    protected override OverwriteExistingPunchItemAttachmentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!, null!, null!);
}
