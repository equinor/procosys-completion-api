using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunch;

[TestClass]
public class GetPunchByGuidQueryHandlerTests : ReadOnlyTestsBase
{
    private Punch _createdPunch;
    private Guid _createdPunchGuid;
    private Punch _modifiedPunch;
    private Guid _modifiedPunchGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _createdPunch = new Punch(TestPlantA, _projectA, "TitleA");
        _modifiedPunch = new Punch(TestPlantA, _projectA, "TitleB");

        context.PunchItems.Add(_createdPunch);
        context.PunchItems.Add(_modifiedPunch);
        context.SaveChangesAsync().Wait();
        _createdPunchGuid = _createdPunch.Guid;

        _modifiedPunch.Update("Modified");
        context.SaveChangesAsync().Wait();
        _modifiedPunchGuid = _modifiedPunch.Guid;
    }

    [TestMethod]
    public async Task Handler_ShouldReturnNotFound_IfPunchIsNotFound()
    {
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        var query = new GetPunchQuery(Guid.Empty);
        var dut = new GetPunchQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.NotFound, result.ResultType);
        Assert.IsNull(result.Data);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectCreatedPunch()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            
        var query = new GetPunchQuery(_createdPunchGuid);
        var dut = new GetPunchQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var punchDetailsDto = result.Data;
        AssertPunch(punchDetailsDto, _createdPunch);
        Assert.IsNull(punchDetailsDto.ModifiedBy);
        Assert.IsNull(punchDetailsDto.ModifiedAtUtc);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectModifiedPunch()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        var query = new GetPunchQuery(_modifiedPunchGuid);
        var dut = new GetPunchQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var punchDetailsDto = result.Data;
        AssertPunch(punchDetailsDto, _modifiedPunch);
        var modifiedBy = punchDetailsDto.ModifiedBy;
        Assert.IsNotNull(modifiedBy);
        Assert.AreEqual(CurrentUserOid, modifiedBy.Guid);
        Assert.IsNotNull(punchDetailsDto.ModifiedAtUtc);
        Assert.AreEqual(_modifiedPunch.ModifiedAtUtc, punchDetailsDto.ModifiedAtUtc);
    }

    private void AssertPunch(PunchDetailsDto punchDetailsDto, Punch punch)
    {
        Assert.AreEqual(punch.ItemNo, punchDetailsDto.ItemNo);
        var project = GetProjectById(punch.ProjectId);
        Assert.AreEqual(project.Name, punchDetailsDto.ProjectName);

        var createdBy = punchDetailsDto.CreatedBy;
        Assert.IsNotNull(createdBy);
        Assert.AreEqual(CurrentUserOid, createdBy.Guid);
        Assert.AreEqual(punch.CreatedAtUtc, punchDetailsDto.CreatedAtUtc);
    }
}
