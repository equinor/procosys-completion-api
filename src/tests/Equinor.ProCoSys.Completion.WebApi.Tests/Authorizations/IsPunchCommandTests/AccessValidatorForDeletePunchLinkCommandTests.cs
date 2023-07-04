using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchCommandTests;

[TestClass]
public class AccessValidatorForDeletePunchItemLinkCommandTests : AccessValidatorForIIsPunchCommandTests<DeletePunchItemLinkCommand>
{
    protected override DeletePunchItemLinkCommand GetPunchItemCommandWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, Guid.Empty, null!);

    protected override DeletePunchItemLinkCommand GetPunchItemCommandWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, Guid.Empty, null!);
}
