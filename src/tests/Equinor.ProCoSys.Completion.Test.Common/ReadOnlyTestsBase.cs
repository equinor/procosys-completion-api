using System;
using System.Linq;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Infrastructure;

namespace Equinor.ProCoSys.Completion.Test.Common;

public abstract class ReadOnlyTestsBase : TestsBase
{
    protected readonly string ProjectNameA = "ProA";
    protected readonly string ProjectNameB = "ProB";
    protected readonly string ProjectNameC = "ProC";
    protected static readonly Guid ProjectGuidA = Guid.NewGuid();
    protected static readonly Guid ProjectGuidB = Guid.NewGuid();
    protected static readonly Guid ProjectGuidC = Guid.NewGuid();
    protected Project _projectA;
    protected Project _projectB;
    protected Project _closedProjectC;
    protected Person _currentPerson;
    protected readonly Guid CurrentUserOid = new ("12345678-1234-1234-1234-123456789123");
    protected DbContextOptions<CompletionContext> _dbContextOptions;
    protected IPlantProvider _plantProvider;
    protected ICurrentUserProvider _currentUserProvider;
    protected IEventDispatcher _eventDispatcher;

    [TestInitialize]
    public void SetupBase()
    {
        _plantProvider = _plantProviderMock.Object;

        var currentUserProviderMock = new Mock<ICurrentUserProvider>();
        currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Returns(CurrentUserOid);
        _currentUserProvider = currentUserProviderMock.Object;

        var eventDispatcher = new Mock<IEventDispatcher>();
        _eventDispatcher = eventDispatcher.Object;

        _dbContextOptions = new DbContextOptionsBuilder<CompletionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            
        // ensure current user exists in db. Will be used when setting createdby / modifiedby
        if (context.Persons.SingleOrDefault(p => p.Guid == CurrentUserOid) is null)
        {
            _currentPerson = new Person(CurrentUserOid, "Ole", "Lukkøye", "ol", "ol@pcs.pcs");
            AddPerson(context, _currentPerson);
        }

        _projectA = new(TestPlantA, ProjectGuidA, ProjectNameA, $"{ProjectNameA} desc");
        _projectB = new(TestPlantA, ProjectGuidB, ProjectNameB, $"{ProjectNameB} desc");
        _closedProjectC = new(TestPlantA, ProjectGuidC, ProjectNameC, $"{ProjectNameC} desc") {IsClosed = true};

        AddProject(context, _projectA);
        AddProject(context, _projectB);
        AddProject(context, _closedProjectC);

        SetupNewDatabase(_dbContextOptions);
    }

    protected abstract void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions);

    protected Project GetProjectById(int projectId)
    {
        using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        return context.Projects.Single(x => x.Id == projectId);
    }

    protected Person AddPerson(CompletionContext context, Person person)
    {
        context.Persons.Add(person);
        context.SaveChangesAsync().Wait();
        return person;
    }

    protected Project AddProject(CompletionContext context, Project project)
    {
        context.Projects.Add(project);
        context.SaveChangesAsync().Wait();
        return project;
    }
}
