using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommentCommandTests : AccessValidatorForCommandNeedAccessTests<CreatePunchItemCommentCommand>
{
    protected override CreatePunchItemCommentCommand GetCommandWithAccessToBothProjectAndContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectAndContent };

    protected override CreatePunchItemCommentCommand GetCommandWithAccessToProjectButNotContent()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithAccessToProjectButNotContent };

    protected override CreatePunchItemCommentCommand GetCommandWithoutAccessToProject()
        => new(Guid.Empty, null!, null!, null!) { PunchItem = PunchItemWithoutAccessToProject };
}
