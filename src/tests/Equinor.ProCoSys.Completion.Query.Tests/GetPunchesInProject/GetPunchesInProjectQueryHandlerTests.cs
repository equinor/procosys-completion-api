using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunchesInProject;

[TestClass]
public class GetPunchesInProjectQueryHandlerTests : ReadOnlyTestsBase
{
    private Punch _nonVoidedPunchInProjectA;
    private Punch _voidedPunchInProjectA;
    private Punch _punchInProjectB;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _nonVoidedPunchInProjectA = new Punch(TestPlantA, _projectA, "NonVoidedA");
        _voidedPunchInProjectA = new Punch(TestPlantA, _projectA, "VoidedA") { IsVoided = true };
        _punchInProjectB = new Punch(TestPlantA, _projectB, "B");

        context.Punches.Add(_nonVoidedPunchInProjectA);
        context.Punches.Add(_voidedPunchInProjectA);
        context.Punches.Add(_punchInProjectB);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_IfNoneFound()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        var query = new GetPunchesInProjectQuery(Guid.Empty);
        var dut = new GetPunchesInProjectQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectPunches()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        var query = new GetPunchesInProjectQuery(_projectA.Guid, true);
        var dut = new GetPunchesInProjectQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(2, result.Data.Count());

        AssertPunch(result.Data.Single(f => !f.IsVoided), _nonVoidedPunchInProjectA);
        AssertPunch(result.Data.Single(f => f.IsVoided), _voidedPunchInProjectA);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnNonVoidedPunches()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        var query = new GetPunchesInProjectQuery(_projectA.Guid);
        var dut = new GetPunchesInProjectQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(1, result.Data.Count());

        AssertPunch(result.Data.Single(f => !f.IsVoided), _nonVoidedPunchInProjectA);
    }

    private void AssertPunch(PunchDto punchDto, Punch punch)
    {
        Assert.AreEqual(punch.Title, punchDto.Title);
        var project = GetProjectById(punch.ProjectId);
        Assert.AreEqual(project.Name, punchDto.ProjectName);
    }
}
