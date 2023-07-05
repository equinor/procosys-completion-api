using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunchItemsInProject;

[TestClass]
public class GetPunchItemsInProjectQueryHandlerTests : ReadOnlyTestsBase
{
    private PunchItem _punchItemInProjectA;
    private PunchItem _punchItemInProjectB;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _punchItemInProjectA = new PunchItem(TestPlantA, _projectA, "A");
        _punchItemInProjectB = new PunchItem(TestPlantA, _projectB, "B");

        context.PunchItems.Add(_punchItemInProjectA);
        context.PunchItems.Add(_punchItemInProjectB);
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

        AssertPunchItem(result.Data.Single(), _punchItemInProjectA);
    }

    private void AssertPunchItem(PunchItemDto punchItemDto, PunchItem punchItem)
    {
        Assert.AreEqual(punchItem.ItemNo, punchItemDto.ItemNo);
        var project = GetProjectById(punchItem.ProjectId);
        Assert.AreEqual(project.Name, punchItemDto.ProjectName);
    }
}
