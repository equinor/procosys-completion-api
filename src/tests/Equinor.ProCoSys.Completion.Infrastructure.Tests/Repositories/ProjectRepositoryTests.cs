using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class ProjectRepositoryTests : EntityWithGuidRepositoryTestBase<Project>
{
    private new ProjectRepository _dut;

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var project = new Project(TestPlant, Guid.NewGuid(), "P", "Description of project", DateTime.Now);
        _knownGuid = project.Guid;
        project.SetProtectedIdForTesting(_knownId);

        var projects = new List<Project> { project };

        _dbSetMock = projects.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Projects
            .Returns(_dbSetMock);

        _dut = new ProjectRepository(_contextHelper.ContextMock);
        base._dut = _dut;
    }

    protected override Project GetNewEntity() => new(TestPlant, Guid.NewGuid(), "New Project", "D", DateTime.Now);
}
