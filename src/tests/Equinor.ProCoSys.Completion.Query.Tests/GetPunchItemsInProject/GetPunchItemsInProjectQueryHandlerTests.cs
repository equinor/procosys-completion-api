using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchItemsInProject;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunchItemsInProject;

[TestClass]
public class GetPunchItemsInProjectQueryHandlerTests : ReadOnlyTestsBase
{
    private Punch _punchInProjectA;
    private Punch _punchInProjectB;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _punchInProjectA = new Punch(TestPlantA, _projectA, "A");
        _punchInProjectB = new Punch(TestPlantA, _projectB, "B");

        context.PunchItems.Add(_punchInProjectA);
        context.PunchItems.Add(_punchInProjectB);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_IfNoneFound()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        var query = new GetPunchItemsInProjectQuery(Guid.Empty);
        var dut = new GetPunchItemsInProjectQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectPunchItems()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        var query = new GetPunchItemsInProjectQuery(_projectA.Guid);
        var dut = new GetPunchItemsInProjectQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(1, result.Data.Count());

        AssertPunch(result.Data.Single(), _punchInProjectA);
    }

    private void AssertPunch(PunchDto punchDto, Punch punch)
    {
        Assert.AreEqual(punch.ItemNo, punchDto.ItemNo);
        var project = GetProjectById(punch.ProjectId);
        Assert.AreEqual(project.Name, punchDto.ProjectName);
    }
}
