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
    protected AccessValidator _dut;
    protected readonly Guid PunchItemGuidWithAccessToProject = new("11111111-1111-1111-1111-111111111111");
    protected readonly Guid PunchItemGuidWithoutAccessToProject = new("22222222-2222-2222-2222-222222222222");
    protected readonly Guid ProjectGuidWithAccess = new("33333333-3333-3333-3333-333333333333");
    protected readonly Guid ProjectGuidWithoutAccess = new("44444444-4444-4444-4444-444444444444");

    private IProjectAccessChecker _projectAccessCheckerMock;
    private ILogger<AccessValidator> _loggerMock;
    private ICurrentUserProvider _currentUserProviderMock;

    [TestInitialize]
    public void Setup()
    {
        _currentUserProviderMock = Substitute.For<ICurrentUserProvider>();

        _projectAccessCheckerMock = Substitute.For<IProjectAccessChecker>();

        _projectAccessCheckerMock.HasCurrentUserAccessToProject(ProjectGuidWithoutAccess).Returns(false);
        _projectAccessCheckerMock.HasCurrentUserAccessToProject(ProjectGuidWithAccess).Returns(true);

        var punchItemHelperMock = Substitute.For<IPunchItemHelper>();
        punchItemHelperMock.GetProjectGuidForPunchItemAsync(PunchItemGuidWithAccessToProject)
            .Returns(ProjectGuidWithAccess);
        punchItemHelperMock.GetProjectGuidForPunchItemAsync(PunchItemGuidWithoutAccessToProject)
            .Returns(ProjectGuidWithoutAccess);

        _loggerMock = Substitute.For<ILogger<AccessValidator>>();

        _dut = new AccessValidator(
            _currentUserProviderMock,
            _projectAccessCheckerMock,
            punchItemHelperMock,
            _loggerMock);
    }
}
