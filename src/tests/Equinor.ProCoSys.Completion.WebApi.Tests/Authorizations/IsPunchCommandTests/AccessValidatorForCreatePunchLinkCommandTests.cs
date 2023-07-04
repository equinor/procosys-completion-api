using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemLinkCommandTests : AccessValidatorForIIsPunchCommandTests<CreatePunchItemLinkCommand>
{
    protected override CreatePunchItemLinkCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!, null!);

    protected override CreatePunchItemLinkCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!, null!);
}
