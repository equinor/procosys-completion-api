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
    protected readonly Guid PunchItemGuidWithAccessToProject = new("11111111-1111-1111-1111-111111111111");
    protected readonly Guid PunchItemGuidWithoutAccessToProject = new("22222222-2222-2222-2222-222222222222");
    protected readonly Guid ProjectGuidWithAccess = new("33333333-3333-3333-3333-333333333333");
    protected readonly Guid ProjectGuidWithoutAccess = new("44444444-4444-4444-4444-444444444444");
    protected readonly Guid CheckListGuidWithAccessToContent = new("55555555-5555-5555-5555-555555555555");
    protected readonly Guid CheckListGuidWithoutAccessToContent = new("66666666-6666-6666-6666-666666666666");

    private Mock<IProjectAccessChecker> _projectAccessCheckerMock;
    private Mock<IContentAccessChecker> _contentAccessCheckerMock;
    private Mock<ILogger<AccessValidator>> _loggerMock;
    private Mock<ICurrentUserProvider> _currentUserProviderMock;

    [TestInitialize]
    public void Setup()
    {
        _currentUserProviderMock = new Mock<ICurrentUserProvider>();

        _projectAccessCheckerMock = new Mock<IProjectAccessChecker>();

        _projectAccessCheckerMock.Setup(p =>
                p.HasCurrentUserAccessToProject(ProjectGuidWithoutAccess))
            .Returns(false);
        _projectAccessCheckerMock.Setup(p =>
                p.HasCurrentUserAccessToProject(ProjectGuidWithAccess))
            .Returns(true);

        _contentAccessCheckerMock = new Mock<IContentAccessChecker>();
        _contentAccessCheckerMock.Setup(c =>
                c.HasCurrentUserAccessToCheckListAsync(CheckListGuidWithoutAccessToContent))
            .ReturnsAsync(false);
        _contentAccessCheckerMock.Setup(c =>
                c.HasCurrentUserAccessToCheckListAsync(CheckListGuidWithAccessToContent))
            .ReturnsAsync(true);

        var punchItemHelperMock = new Mock<IPunchItemHelper>();
        punchItemHelperMock.Setup(p => p.GetProjectGuidForPunchItemAsync(PunchItemGuidWithAccessToProject))
            .ReturnsAsync(ProjectGuidWithAccess);
        punchItemHelperMock.Setup(p => p.GetProjectGuidForPunchItemAsync(PunchItemGuidWithoutAccessToProject))
            .ReturnsAsync(ProjectGuidWithoutAccess);

        _loggerMock = new Mock<ILogger<AccessValidator>>();

        _dut = new AccessValidator(
            _currentUserProviderMock.Object,
            _projectAccessCheckerMock.Object,
            _contentAccessCheckerMock.Object,
            punchItemHelperMock.Object,
            _loggerMock.Object);
    }
}
