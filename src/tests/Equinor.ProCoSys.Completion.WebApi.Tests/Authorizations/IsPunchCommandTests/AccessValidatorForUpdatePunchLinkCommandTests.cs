using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForUpdatePunchItemLinkCommandTests : AccessValidatorForIIsPunchCommandTests<UpdatePunchItemLinkCommand>
{
    protected override UpdatePunchItemLinkCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, Guid.Empty, null!, null!, null!);

    protected override UpdatePunchItemLinkCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, Guid.Empty, null!, null!, null!);
}
