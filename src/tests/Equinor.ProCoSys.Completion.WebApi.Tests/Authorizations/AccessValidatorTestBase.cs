using System;
using System.Threading;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public abstract class AccessValidatorTestBase
{
    protected static string Plant = "P";

    protected static LibraryItem Org = new(Plant, Guid.Empty, null!, null!, LibraryType.COMPLETION_ORGANIZATION);

    protected static Guid CheckListGuidWithAccessToContent = new("55555555-5555-5555-5555-555555555555");
    protected static Guid CheckListGuidWithAccessToProjectAndContent = new("99999999-9999-9999-9999-999999999999");
    protected static Guid CheckListGuidWithoutAccessToContent = new("66666666-6666-6666-6666-666666666666");

    protected static Guid ProjectGuidWithAccess = new("33333333-3333-3333-3333-333333333333");
    private static readonly Project s_projectWithAccess = new(Plant, ProjectGuidWithAccess, null!, null!);
    protected static PunchItem PunchItemWithAccessToProjectAndContent
        = new(Plant, s_projectWithAccess, CheckListGuidWithAccessToProjectAndContent, Category.PA, null!, Org, Org);

    protected static Guid ProjectGuidWithoutAccess = new("44444444-4444-4444-4444-444444444444");
    private static readonly Project s_projectWithoutAccess = new(Plant, ProjectGuidWithoutAccess, null!, null!);
    protected static PunchItem PunchItemWithoutAccessToProject
        = new(Plant, s_projectWithoutAccess, CheckListGuidWithAccessToContent, Category.PA, null!, Org, Org);

    protected static PunchItem PunchItemWithAccessToProjectButNotContent
        = new(Plant, s_projectWithAccess, CheckListGuidWithoutAccessToContent, Category.PA, null!, Org, Org);

    protected static CheckListDetailsDto CheckListWithAccessToBothProjectAndContent =
        new(CheckListGuidWithAccessToContent, "R", false, ProjectGuidWithAccess);
    protected static CheckListDetailsDto CheckListWithAccessToProjectButNotContent =
        new(CheckListGuidWithoutAccessToContent, "R", false, ProjectGuidWithAccess);
    protected static CheckListDetailsDto CheckListWithoutAccessToProject =
        new(Guid.NewGuid(), "R", false, ProjectGuidWithoutAccess);

    protected AccessValidator _dut = null!;

    [TestInitialize]
    public void Setup()
    {
        var projectAccessCheckerMock = Substitute.For<IProjectAccessChecker>();
        projectAccessCheckerMock.HasCurrentUserAccessToProject(ProjectGuidWithoutAccess).Returns(false);
        projectAccessCheckerMock.HasCurrentUserAccessToProject(ProjectGuidWithAccess).Returns(true);

        var accessCheckerMock = Substitute.For<IAccessChecker>();
        accessCheckerMock.HasCurrentUserWriteAccessToCheckListAsync(CheckListGuidWithoutAccessToContent, Arg.Any<CancellationToken>())
            .Returns(false);
        accessCheckerMock.HasCurrentUserWriteAccessToCheckListAsync(CheckListGuidWithAccessToContent, Arg.Any<CancellationToken>())
            .Returns(true);
        accessCheckerMock.HasCurrentUserWriteAccessToCheckListAsync(CheckListGuidWithAccessToProjectAndContent, Arg.Any<CancellationToken>())
            .Returns(true);
        accessCheckerMock.HasCurrentUserWriteAccessToCheckList(CheckListWithAccessToProjectButNotContent)
            .Returns(false);
        accessCheckerMock.HasCurrentUserWriteAccessToCheckList(CheckListWithAccessToBothProjectAndContent)
            .Returns(true);
        accessCheckerMock.HasCurrentUserWriteAccessToCheckList(CheckListWithoutAccessToProject)
            .Returns(true);
        _dut = new AccessValidator(
            Substitute.For<ICurrentUserProvider>(),
            projectAccessCheckerMock,
            accessCheckerMock,
            Substitute.For<ILogger<AccessValidator>>());
    }
}
