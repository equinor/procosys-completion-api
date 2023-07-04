using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunchItem;

[TestClass]
public class GetPunchItemQueryHandlerTests : ReadOnlyTestsBase
{
    private PunchItem _createdPunch;
    private Guid _createdPunchItemGuid;
    private PunchItem _modifiedPunch;
    private Guid _modifiedPunchItemGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _createdPunch = new PunchItem(TestPlantA, _projectA, "TitleA");
        _modifiedPunch = new PunchItem(TestPlantA, _projectA, "TitleB");

        context.PunchItems.Add(_createdPunch);
        context.PunchItems.Add(_modifiedPunch);
        context.SaveChangesAsync().Wait();
        _createdPunchItemGuid = _createdPunch.Guid;

        _modifiedPunch.Update("Modified");
        context.SaveChangesAsync().Wait();
        _modifiedPunchItemGuid = _modifiedPunch.Guid;
    }

    [TestMethod]
    public async Task Handler_ShouldReturnNotFound_IfPunchIsNotFound()
    {
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        var query = new GetPunchItemQuery(Guid.Empty);
        var dut = new GetPunchItemQueryHandler(context);

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
            
        var query = new GetPunchItemQuery(_createdPunchItemGuid);
        var dut = new GetPunchItemQueryHandler(context);

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

        var query = new GetPunchItemQuery(_modifiedPunchItemGuid);
        var dut = new GetPunchItemQueryHandler(context);

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

    private void AssertPunch(PunchItemDetailsDto punchDetailsDto, PunchItem punchItem)
    {
        Assert.AreEqual(punchItem.ItemNo, punchDetailsDto.ItemNo);
        var project = GetProjectById(punchItem.ProjectId);
        Assert.AreEqual(project.Name, punchDetailsDto.ProjectName);

        var createdBy = punchDetailsDto.CreatedBy;
        Assert.IsNotNull(createdBy);
        Assert.AreEqual(CurrentUserOid, createdBy.Guid);
        Assert.AreEqual(punchItem.CreatedAtUtc, punchDetailsDto.CreatedAtUtc);
    }
}
