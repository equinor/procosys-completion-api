﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.ProjectQueries.GetPunchItems;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Query.Tests.ProjectQueries.GetPunchItems;

[TestClass]
public class GetPunchItemsQueryHandlerTests : ReadOnlyTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private PunchItem _punchItemInProjectA;
    private PunchItem _punchItemInProjectB;
    private Project _projectA;
    private Project _projectB;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _projectA = context.Projects.Single(p => p.Id == _projectAId[_testPlant]);
        _projectB = context.Projects.Single(p => p.Id == _projectBId[_testPlant]);
        var raisedByOrg = context.Library.Single(l => l.Id == _raisedByOrgId[_testPlant]);
        var clearingByOrg = context.Library.Single(l => l.Id == _clearingByOrgId[_testPlant]);

        _punchItemInProjectA = new PunchItem(_testPlant, _projectA, Guid.NewGuid(), Category.PA, "A", raisedByOrg, clearingByOrg);
        _punchItemInProjectB = new PunchItem(_testPlant, _projectB, Guid.NewGuid(), Category.PA, "B", raisedByOrg, clearingByOrg);

        context.PunchItems.Add(_punchItemInProjectA);
        context.PunchItems.Add(_punchItemInProjectB);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_IfNoneFound()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var query = new GetPunchItemsQuery(Guid.Empty);
        var dut = new GetPunchItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectPunchItems()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var query = new GetPunchItemsQuery(_projectA.Guid);
        var dut = new GetPunchItemsQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());

        AssertPunchItem(result.Single(), _punchItemInProjectA);
    }

    private void AssertPunchItem(PunchItemDto punchItemDto, PunchItem punchItem)
    {
        Assert.AreEqual(punchItem.ItemNo, punchItemDto.ItemNo);
        Assert.AreEqual(punchItem.Category, punchItemDto.Category);
        Assert.AreEqual(punchItem.Description, punchItemDto.Description);
        Assert.AreEqual(punchItem.RowVersion.ConvertToString(), punchItemDto.RowVersion);
        var project = GetProjectById(punchItem.ProjectId);
        Assert.AreEqual(project.Name, punchItemDto.ProjectName);
    }
}
