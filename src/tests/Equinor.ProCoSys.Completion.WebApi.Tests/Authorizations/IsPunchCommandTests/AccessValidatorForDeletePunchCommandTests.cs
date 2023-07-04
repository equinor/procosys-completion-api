using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemCommandTests : AccessValidatorForIIsPunchCommandTests<DeletePunchItemCommand>
{
    protected override DeletePunchItemCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, null!);

    protected override DeletePunchItemCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null!);
}
