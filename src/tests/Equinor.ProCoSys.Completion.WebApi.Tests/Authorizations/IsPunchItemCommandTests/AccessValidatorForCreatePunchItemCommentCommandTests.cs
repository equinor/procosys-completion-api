using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommentCommandTests : AccessValidatorForIIsPunchItemCommandTests<CreatePunchItemCommentCommand>
{
    protected override CreatePunchItemCommentCommand GetPunchItemCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override CreatePunchItemCommentCommand GetPunchItemCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override CreatePunchItemCommentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
