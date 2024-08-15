using System;
using System.Threading;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class AccessValidatorTestBase
{
    protected static string Plant = "P";

    protected static Guid CheckListGuidWithAccessToContent = new("55555555-5555-5555-5555-555555555555");
    protected static Guid CheckListGuidWithAccessToProjectAndContent = new("99999999-9999-9999-9999-999999999999");
    protected static Guid CheckListGuidWithoutAccessToContent = new("66666666-6666-6666-6666-666666666666");
    protected static Guid CheckListGuidWithoutAccessToProject = new("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");

    protected static Guid ProjectGuidWithAccess = new("33333333-3333-3333-3333-333333333333");
    protected static Project ProjectWithAccess = new(Plant, ProjectGuidWithAccess, null!, null!);
    protected static PunchItem PunchItemWithAccessToProjectAndContent
        = new(Plant, ProjectWithAccess, CheckListGuidWithAccessToProjectAndContent, Category.PA, null!, null!, null!);

    protected static Guid ProjectGuidWithoutAccess = new("44444444-4444-4444-4444-444444444444");
    protected static Project ProjectWithoutAccess = new(Plant, ProjectGuidWithoutAccess, null!, null!);
    protected static PunchItem PunchItemWithoutAccessToProject
        = new(Plant, ProjectWithoutAccess, CheckListGuidWithAccessToContent, Category.PA, null!, null!, null!);

    protected static PunchItem PunchItemWithAccessToProjectButNotContent
        = new(Plant, ProjectWithAccess, CheckListGuidWithoutAccessToContent, Category.PA, null!, null!, null!);

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

        var checkListCacheMock = Substitute.For<ICheckListCache>();
        checkListCacheMock.GetCheckListAsync(CheckListGuidWithoutAccessToProject, Arg.Any<CancellationToken>())
            .Returns(new ProCoSys4CheckList(null!, false, ProjectGuidWithoutAccess));

        _dut = new AccessValidator(
            Substitute.For<ICurrentUserProvider>(),
            projectAccessCheckerMock,
            accessCheckerMock,
            checkListCacheMock,
            Substitute.For<ILogger<AccessValidator>>());
    }
}
