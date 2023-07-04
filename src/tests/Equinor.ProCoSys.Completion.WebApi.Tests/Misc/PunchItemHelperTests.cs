using System;
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

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
                
        // Save to get real id on project
        context.SaveChangesAsync().Wait();

        var punchItem = new PunchItem(TestPlantA, _projectA, "Title");
        context.PunchItems.Add(punchItem);
        context.SaveChangesAsync().Wait();
        _punchItemGuid = punchItem.Guid;
    }

    [TestMethod]
    public async Task GetProjectGuidForPunchAsync_KnownPunchId_ShouldReturnProjectGuid()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemHelper(context);

        // Act
        var projectGuid = await dut.GetProjectGuidForPunchAsync(_punchItemGuid);

        // Assert
        Assert.AreEqual(_projectA.Guid, projectGuid);
    }

    [TestMethod]
    public async Task GetProjectGuidForPunchAsync_UnKnownPunchId_ShouldReturnNull()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchItemHelper(context);

        // Act
        var projectGuid = await dut.GetProjectGuidForPunchAsync(Guid.Empty);

        // Assert
        Assert.IsNull(projectGuid);
    }
}
