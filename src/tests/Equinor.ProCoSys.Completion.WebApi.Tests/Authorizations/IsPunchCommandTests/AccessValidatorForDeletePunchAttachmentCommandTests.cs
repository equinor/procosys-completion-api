using System;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchAttachmentCommandTests : AccessValidatorForIIsPunchCommandTests<DeletePunchAttachmentCommand>
{
    protected override DeletePunchAttachmentCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, Guid.Empty, null!);

    protected override DeletePunchAttachmentCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, Guid.Empty, null!);
}
