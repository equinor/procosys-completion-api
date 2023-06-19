using Equinor.ProCoSys.Completion.Command.PunchCommands.UploadNewPunchAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForUploadNewPunchAttachmentCommandTests : AccessValidatorForIIsPunchCommandTests<UploadNewPunchAttachmentCommand>
{
    protected override UploadNewPunchAttachmentCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, null!, null!);

    protected override UploadNewPunchAttachmentCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, null!, null!);
}
