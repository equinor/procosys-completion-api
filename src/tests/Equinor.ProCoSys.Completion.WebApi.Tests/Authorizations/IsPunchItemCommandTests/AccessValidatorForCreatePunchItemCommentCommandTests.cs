using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommentCommandTests : AccessValidatorForIIsPunchItemCommandTests<CreatePunchItemCommentCommand>
{
    protected override CreatePunchItemCommentCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!);

    protected override CreatePunchItemCommentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!);
}
