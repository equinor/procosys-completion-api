using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class PunchItemRepositoryTests : EntityWithGuidRepositoryTestBase<PunchItem>
{
    private Project _project;
    private LibraryItem _raisedByOrg;
    private LibraryItem _clearingByOrg;
    private LibraryItem _priority;
    private LibraryItem _sorting;
    private LibraryItem _type;

    protected override EntityWithGuidRepository<PunchItem> GetDut() => new PunchItemRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        _project = new Project(TestPlant, Guid.NewGuid(), null!, null!, DateTime.Now);
        _raisedByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _clearingByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        var punchItem = new PunchItem(TestPlant, _project, Guid.NewGuid(), Category.PB, null!, _raisedByOrg, _clearingByOrg);
        _priority = new LibraryItem(TestPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_PRIORITY);
        punchItem.SetPriority(_priority);
        _sorting = new LibraryItem(TestPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_SORTING);
        punchItem.SetSorting(_sorting);
        _type = new LibraryItem(TestPlant, Guid.NewGuid(), null!, null!, LibraryType.PUNCHLIST_TYPE);
        punchItem.SetType(_type);

        _knownGuid = punchItem.Guid;
        punchItem.SetProtectedIdForTesting(_knownId);

        var punchItems = new List<PunchItem> { punchItem };

        _dbSetMock = punchItems.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .PunchItems
            .Returns(_dbSetMock);
    }

    protected override PunchItem GetNewEntity() => new(TestPlant, _project, Guid.NewGuid(), Category.PA, null!, _raisedByOrg, _clearingByOrg);

    [TestMethod]
    public async Task GetAsync_KnownGuid_ShouldReturnEntityWithNavigationProperties()
    {
        // Act
        var result = await GetDut().GetAsync(_knownGuid, default);

        // Assert
        Assert.IsNotNull(result);

        Assert.IsNotNull(result.Project);
        Assert.AreEqual(_project.Guid, result.Project.Guid);

        Assert.IsNotNull(result.RaisedByOrg);
        Assert.AreEqual(_raisedByOrg.Guid, result.RaisedByOrg.Guid);
        
        Assert.IsNotNull(result.ClearingByOrg);
        Assert.AreEqual(_clearingByOrg.Guid, result.ClearingByOrg.Guid);

        Assert.IsNotNull( result.Priority);
        Assert.AreEqual(_priority.Guid, result.Priority.Guid);

        Assert.IsNotNull(result.Sorting);
        Assert.AreEqual(_sorting.Guid, result.Sorting.Guid);

        Assert.IsNotNull(result.Type);
        Assert.AreEqual(_type.Guid, result.Type.Guid);
    }
}
