using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectCommandTests;

[TestClass]
public class AccessValidatorForCreatePunchItemCommandTests : AccessValidatorForIIsProjectCommandTests<CreatePunchItemCommand>
{
    protected override CreatePunchItemCommand GetProjectCommandWithAccessToBothProjectAndContent()
        => new(null!, ProjectGuidWithAccess, CheckListGuidWithAccessToContent, Guid.Empty, Guid.Empty);

    protected override CreatePunchItemCommand GetProjectCommandWithAccessToProjectButNotContent()
        => new(null!, 
            ProjectGuidWithAccess, 
            CheckListGuidWithoutAccessToContent,
            Guid.Empty,
            Guid.Empty);

    protected override CreatePunchItemCommand GetProjectCommandWithoutAccessToProject()
        => new(null!, ProjectGuidWithoutAccess, Guid.Empty, Guid.Empty, Guid.Empty);
}
