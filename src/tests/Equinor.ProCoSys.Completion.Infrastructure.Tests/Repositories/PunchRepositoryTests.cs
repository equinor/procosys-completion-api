using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class PunchRepositoryTests : EntityWithGuidRepositoryTestBase<Punch>
{
    private Project _project;

    protected override void SetupRepositoryWithOneKnownItem()
    {
        _project = new Project(TestPlant, Guid.NewGuid(), "ProjectName", "Description of project");
        var punch = new Punch(TestPlant, _project, "Punch X");
        _knownGuid = punch.Guid;
        punch.SetProtectedIdForTesting(_knownId);

        var punchItems = new List<Punch> { punch };

        _dbSetMock = punchItems.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Setup(x => x.PunchItems)
            .Returns(_dbSetMock.Object);

        _dut = new PunchRepository(_contextHelper.ContextMock.Object);
    }

    protected override Punch GetNewEntity() => new(TestPlant, _project, "New punch");
}
