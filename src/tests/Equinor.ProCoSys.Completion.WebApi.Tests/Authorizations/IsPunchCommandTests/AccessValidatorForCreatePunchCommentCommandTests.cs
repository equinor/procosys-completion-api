using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchComment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchCommentCommandTests : AccessValidatorForIPunchCommandTests<CreatePunchCommentCommand>
{
    protected override CreatePunchCommentCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, null!);

    protected override CreatePunchCommentCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, null!);
}
