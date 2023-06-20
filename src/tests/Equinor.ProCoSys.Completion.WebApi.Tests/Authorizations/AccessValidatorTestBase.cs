using System;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Equinor.ProCoSys.Common.Misc;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class AccessValidatorTestBase
{
    protected AccessValidator _dut;
    protected readonly Guid PunchGuidWithAccessToProject = new("11111111-1111-1111-1111-111111111111");
    protected readonly Guid PunchGuidWithoutAccessToProject = new("22222222-2222-2222-2222-222222222222");
    protected readonly Guid ProjectGuidWithAccess = new("33333333-3333-3333-3333-333333333333");
    protected readonly Guid ProjectGuidWithoutAccess = new("44444444-4444-4444-4444-444444444444");

    private Mock<IProjectAccessChecker> _projectAccessCheckerMock;
    private Mock<ILogger<AccessValidator>> _loggerMock;
    private Mock<ICurrentUserProvider> _currentUserProviderMock;

    [TestInitialize]
    public void Setup()
    {
        _currentUserProviderMock = new Mock<ICurrentUserProvider>();

        _projectAccessCheckerMock = new Mock<IProjectAccessChecker>();

        _projectAccessCheckerMock.Setup(p => p.HasCurrentUserAccessToProject(ProjectGuidWithoutAccess)).Returns(false);
        _projectAccessCheckerMock.Setup(p => p.HasCurrentUserAccessToProject(ProjectGuidWithAccess)).Returns(true);

        var punchHelperMock = new Mock<IPunchHelper>();
        punchHelperMock.Setup(p => p.GetProjectGuidForPunchAsync(PunchGuidWithAccessToProject))
            .ReturnsAsync(ProjectGuidWithAccess);
        punchHelperMock.Setup(p => p.GetProjectGuidForPunchAsync(PunchGuidWithoutAccessToProject))
            .ReturnsAsync(ProjectGuidWithoutAccess);

        _loggerMock = new Mock<ILogger<AccessValidator>>();

        _dut = new AccessValidator(
            _currentUserProviderMock.Object,
            _projectAccessCheckerMock.Object,
            punchHelperMock.Object,
            _loggerMock.Object);
    }
}
