using System;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Common.Misc;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class AccessValidatorTestBase
{
    protected AccessValidator _dut = null!;
    protected readonly Guid PunchItemGuidWithAccessToProjectAndContent = new("11111111-1111-1111-1111-111111111111");
    protected readonly Guid PunchItemGuidWithoutAccessToProject = new("22222222-2222-2222-2222-222222222222");
    protected readonly Guid ProjectGuidWithAccess = new("33333333-3333-3333-3333-333333333333");
    protected readonly Guid ProjectGuidWithoutAccess = new("44444444-4444-4444-4444-444444444444");
    protected readonly Guid CheckListGuidWithAccessToContent = new("55555555-5555-5555-5555-555555555555");
    protected readonly Guid CheckListGuidWithoutAccessToContent = new("66666666-6666-6666-6666-666666666666");
    protected readonly Guid PunchItemGuidWithAccessToProjectButNotContent = new("77777777-7777-7777-7777-777777777777");

    private IProjectAccessChecker _projectAccessCheckerMock = null!;
    private IContentAccessChecker _contentAccessCheckerMock = null!;
    private ILogger<AccessValidator> _loggerMock = null!;
    private ICurrentUserProvider _currentUserProviderMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _currentUserProviderMock = Substitute.For<ICurrentUserProvider>();

        _projectAccessCheckerMock = Substitute.For<IProjectAccessChecker>();

        _projectAccessCheckerMock.HasCurrentUserAccessToProject(ProjectGuidWithoutAccess).Returns(false);
        _projectAccessCheckerMock.HasCurrentUserAccessToProject(ProjectGuidWithAccess).Returns(true);

        _contentAccessCheckerMock = Substitute.For<IContentAccessChecker>();
        _contentAccessCheckerMock.HasCurrentUserAccessToCheckListAsync(CheckListGuidWithoutAccessToContent)
            .Returns(false);
        _contentAccessCheckerMock.HasCurrentUserAccessToCheckListAsync(CheckListGuidWithAccessToContent)
            .Returns(true);
        _contentAccessCheckerMock.HasCurrentUserAccessToCheckListOwningPunchItemAsync(PunchItemGuidWithAccessToProjectButNotContent)
            .Returns(false);
        _contentAccessCheckerMock.HasCurrentUserAccessToCheckListOwningPunchItemAsync(PunchItemGuidWithAccessToProjectAndContent)
            .Returns(true);

        var punchItemHelperMock = Substitute.For<IPunchItemHelper>();
        punchItemHelperMock.GetProjectGuidForPunchItemAsync(PunchItemGuidWithAccessToProjectAndContent)
            .Returns(ProjectGuidWithAccess);
        punchItemHelperMock.GetProjectGuidForPunchItemAsync(PunchItemGuidWithAccessToProjectButNotContent)
            .Returns(ProjectGuidWithAccess);
        punchItemHelperMock.GetProjectGuidForPunchItemAsync(PunchItemGuidWithoutAccessToProject)
            .Returns(ProjectGuidWithoutAccess);

        _loggerMock = Substitute.For<ILogger<AccessValidator>>();

        _dut = new AccessValidator(
            _currentUserProviderMock,
            _projectAccessCheckerMock,
            _contentAccessCheckerMock,
            punchItemHelperMock,
            _loggerMock);
    }
}
