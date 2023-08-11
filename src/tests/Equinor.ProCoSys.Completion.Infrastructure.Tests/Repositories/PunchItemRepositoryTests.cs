using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class PunchItemRepositoryTests : EntityWithGuidRepositoryTestBase<PunchItem>
{
    private Project _project;
    private LibraryItem _raisedByOrg;
    private LibraryItem _clearingByOrg;

    protected override void SetupRepositoryWithOneKnownItem()
    {
        _project = new Project(TestPlant, Guid.NewGuid(), null!, null!);
        _raisedByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        _clearingByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), null!, null!, LibraryType.COMPLETION_ORGANIZATION);
        var punchItem = new PunchItem(TestPlant, _project, Guid.NewGuid(), null!, _raisedByOrg, _clearingByOrg);
        _knownGuid = punchItem.Guid;
        punchItem.SetProtectedIdForTesting(_knownId);

        var punchItems = new List<PunchItem> { punchItem };

        _dbSetMock = punchItems.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Setup(x => x.PunchItems)
            .Returns(_dbSetMock.Object);

        _dut = new PunchItemRepository(_contextHelper.ContextMock.Object);
    }

    protected override PunchItem GetNewEntity() => new(TestPlant, _project, Guid.NewGuid(), null!, _raisedByOrg, _clearingByOrg);
}
