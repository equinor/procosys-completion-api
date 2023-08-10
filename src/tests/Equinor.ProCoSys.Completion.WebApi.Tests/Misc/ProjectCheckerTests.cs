using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Equinor.ProCoSys.Completion.Command;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Misc;

[TestClass]
public class ProjectCheckerTests
{
    private readonly Guid _currentUserOid = new("12345678-1234-1234-1234-123456789123");
    private readonly string _plant = "Plant";
    private readonly Guid _projectGuid = Guid.NewGuid();

    private IPlantProvider _plantProviderMock;
    private ICurrentUserProvider _currentUserProviderMock;
    private IPermissionCache _permissionCacheMock;
    private TestRequest _testRequest;
    private ProjectChecker _dut;

    [TestInitialize]
    public void Setup()
    {
        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant.Returns(_plant);

        _currentUserProviderMock = Substitute.For<ICurrentUserProvider>();
        _currentUserProviderMock.GetCurrentUserOid().Returns(_currentUserOid);

        _permissionCacheMock = Substitute.For<IPermissionCache>();

        _testRequest = new TestRequest(_projectGuid);
        _dut = new ProjectChecker(_plantProviderMock, _currentUserProviderMock, _permissionCacheMock);
    }

    [TestMethod]
    public async Task EnsureValidProjectAsync_ShouldValidateOK()
    {
        // Arrange
        _permissionCacheMock.IsAValidProjectForUserAsync(_plant, _currentUserOid, _projectGuid).Returns(true);

        // Act
        await _dut.EnsureValidProjectAsync(_testRequest);
    }

    [TestMethod]
    public async Task EnsureValidProjectAsync_ShouldThrowInvalidException_WhenProjectIsNotValid()
    {
        // Arrange
        _permissionCacheMock.IsAValidProjectForUserAsync(_plant, _currentUserOid, _projectGuid).Returns(false);

        // Act
        await Assert.ThrowsExceptionAsync<InValidProjectException>(() => _dut.EnsureValidProjectAsync(_testRequest));
    }

    [TestMethod]
    public async Task EnsureValidProjectAsync_ShouldThrowException_WhenRequestIsNull()
        => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _dut.EnsureValidProjectAsync((IBaseRequest)null));

    private class TestRequest : IIsProjectCommand
    {
        public TestRequest(Guid projectGuid) => ProjectGuid = projectGuid;

        public Guid ProjectGuid { get; }
    }
}
