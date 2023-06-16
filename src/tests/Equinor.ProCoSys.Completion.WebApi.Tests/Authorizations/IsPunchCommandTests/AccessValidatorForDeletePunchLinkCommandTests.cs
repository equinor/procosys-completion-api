using System;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchLinkCommandTests : AccessValidatorForIPunchCommandTests<DeletePunchLinkCommand>
{
    protected override DeletePunchLinkCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, Guid.Empty, null!);

    protected override DeletePunchLinkCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, Guid.Empty, null!);
}
