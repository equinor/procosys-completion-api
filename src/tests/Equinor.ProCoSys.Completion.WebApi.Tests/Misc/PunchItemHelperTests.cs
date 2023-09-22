using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Misc;

[TestClass]
public class PunchItemHelperTests : ReadOnlyTestsBase
{
    private Guid _punchItemGuid;
    private readonly Guid _checkListGuid = Guid.NewGuid();

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var raisedByOrg = context.Library.Single(l => l.Id == _raisedByOrgId);
        var clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId);

        // Save to get real id on project
        context.SaveChangesAsync().Wait();

        var punchItem = new PunchItem(TestPlantA, _projectA, _checkListGuid, "Title", raisedByOrg, clearingByOrg);
        context.PunchItems.Add(punchItem);
        context.SaveChangesAsync().Wait();
        _punchItemGuid = punchItem.Guid;
    }

    [TestMethod]
    public async Task GetProjectGuidForPunchItem_ShouldReturnProjectGuid_WhenKnownPunchItemId()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemHelper(context);

        // Act
        var projectGuid = await dut.GetProjectGuidForPunchItemAsync(_punchItemGuid);

        // Assert
        Assert.AreEqual(_projectA.Guid, projectGuid);
    }

    [TestMethod]
    public async Task GetProjectGuidForPunchItem_ShouldReturnNull_WhenUnKnownPunchItemId()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemHelper(context);

        // Act
        var projectGuid = await dut.GetProjectGuidForPunchItemAsync(Guid.Empty);

        // Assert
        Assert.IsNull(projectGuid);
    }

    [TestMethod]
    public async Task GetCheckListGuidForPunchItem_ShouldReturnCheckListGuid_WhenKnownPunchItemId()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemHelper(context);

        // Act
        var checkListGuid = await dut.GetCheckListGuidForPunchItemAsync(_punchItemGuid);

        // Assert
        Assert.AreEqual(_checkListGuid, checkListGuid);
    }

    [TestMethod]
    public async Task GetCheckListGuidForPunchItem_ShouldReturnNull_WhenUnKnownPunchItemId()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new PunchItemHelper(context);

        // Act
        var checkListGuid = await dut.GetCheckListGuidForPunchItemAsync(Guid.Empty);

        // Assert
        Assert.IsNull(checkListGuid);
    }
}
