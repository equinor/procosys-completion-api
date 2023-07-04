using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemAttachmentCommandTests : AccessValidatorForIIsPunchCommandTests<DeletePunchItemAttachmentCommand>
{
    protected override DeletePunchItemAttachmentCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, Guid.Empty, null!);

    protected override DeletePunchItemAttachmentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, Guid.Empty, null!);
}
