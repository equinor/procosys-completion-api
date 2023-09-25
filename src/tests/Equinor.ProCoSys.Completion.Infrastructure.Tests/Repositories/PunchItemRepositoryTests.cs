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

    protected override void SetupRepositoryWithOneKnownItem()
    {
        _project = new Project(TestPlant, Guid.NewGuid(), null!, null!);
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

        _dut = new PunchItemRepository(_contextHelper.ContextMock);
    }

    protected override PunchItem GetNewEntity() => new(TestPlant, _project, Guid.NewGuid(), Category.PA, null!, _raisedByOrg, _clearingByOrg);

    [TestMethod]
    public async Task GetByGuid_KnownGuid_ShouldReturnEntityWithLibraryProperties()
    {
        // Act
        var result = await _dut.GetByGuidAsync(_knownGuid);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_raisedByOrg, result.RaisedByOrg);
        Assert.AreEqual(_clearingByOrg, result.ClearingByOrg);
        Assert.AreEqual(_priority, result.Priority);
        Assert.AreEqual(_sorting, result.Sorting);
        Assert.AreEqual(_type, result.Type);
    }
}
