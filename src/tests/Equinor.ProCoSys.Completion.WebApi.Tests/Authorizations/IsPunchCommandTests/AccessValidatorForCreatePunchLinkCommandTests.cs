using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchLinkCommandTests : AccessValidatorForIIsPunchCommandTests<CreatePunchLinkCommand>
{
    protected override CreatePunchLinkCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, null!, null!);

    protected override CreatePunchLinkCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, null!, null!);
}
