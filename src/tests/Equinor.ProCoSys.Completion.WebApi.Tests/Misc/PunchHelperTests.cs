using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Misc;

[TestClass]
public class PunchHelperTests : ReadOnlyTestsBase
{
    private Guid _punchGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
                
        // Save to get real id on project
        context.SaveChangesAsync().Wait();

        var punch = new Punch(TestPlantA, _projectA, "Title");
        context.Punches.Add(punch);
        context.SaveChangesAsync().Wait();
        _punchGuid = punch.Guid;
    }

    [TestMethod]
    public async Task GetProjectGuidForPunchAsync_KnownPunchId_ShouldReturnProjectGuid()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchHelper(context);

        // Act
        var projectGuid = await dut.GetProjectGuidForPunchAsync(_punchGuid);

        // Assert
        Assert.AreEqual(_projectA.Guid, projectGuid);
    }

    [TestMethod]
    public async Task GetProjectGuidForPunchAsync_UnKnownPunchId_ShouldReturnNull()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new PunchHelper(context);

        // Act
        var projectGuid = await dut.GetProjectGuidForPunchAsync(Guid.Empty);

        // Assert
        Assert.IsNull(projectGuid);
    }
}
