using System;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunchLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchLinkCommandTests : AccessValidatorForIIsPunchCommandTests<UpdatePunchLinkCommand>
{
    protected override UpdatePunchLinkCommand GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, Guid.Empty, null!, null!, null!);

    protected override UpdatePunchLinkCommand GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, Guid.Empty, null!, null!, null!);
}
