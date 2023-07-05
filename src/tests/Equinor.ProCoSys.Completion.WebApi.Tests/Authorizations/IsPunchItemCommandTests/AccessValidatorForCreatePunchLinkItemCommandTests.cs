using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemLinkCommandTests : AccessValidatorForIIsPunchItemCommandTests<CreatePunchItemLinkCommand>
{
    protected override CreatePunchItemLinkCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!, null!);

    protected override CreatePunchItemLinkCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!, null!);
}
