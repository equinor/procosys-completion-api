using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemAttachmentCommandTests : AccessValidatorForIIsPunchItemCommandTests<UpdatePunchItemAttachmentCommand>
{
    protected override UpdatePunchItemAttachmentCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(PunchItemGuidWithAccessToProjectAndContent, Guid.Empty, "a", null!, null!);

    protected override UpdatePunchItemAttachmentCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(PunchItemGuidWithAccessToProjectButNotContent, Guid.Empty, "a", null!, null!);

    protected override UpdatePunchItemAttachmentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, Guid.Empty, "a", null!, null!);
}
