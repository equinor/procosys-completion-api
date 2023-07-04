using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommentCommandTests : AccessValidatorForIIsPunchCommandTests<CreatePunchItemCommentCommand>
{
    protected override CreatePunchItemCommentCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!);

    protected override CreatePunchItemCommentCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!);
}
