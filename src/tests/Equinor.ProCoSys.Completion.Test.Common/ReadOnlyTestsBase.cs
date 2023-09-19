using System;
using System.Linq;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Test.Common;

public abstract class ReadOnlyTestsBase : TestsBase
{
    protected Project _projectA;
    protected Project _projectB;
    protected Project _closedProjectC;
    protected Person _currentPerson;
    protected LibraryItem _raisedByOrg;
    protected LibraryItem _clearingByOrg;
    protected LibraryItem _priority;
    protected LibraryItem _sorting;
    protected LibraryItem _type;
    protected readonly Guid CurrentUserOid = new ("12345678-1234-1234-1234-123456789123");
    protected DbContextOptions<CompletionContext> _dbContextOptions;
    protected IPlantProvider _plantProviderMockObject;
    protected ICurrentUserProvider _currentUserProviderMockObject;
    protected IEventDispatcher _eventDispatcherMockObject;

    [TestInitialize]
    public void SetupBase()
    {
        _plantProviderMockObject = _plantProviderMock;

        var currentUserProviderMock = Substitute.For<ICurrentUserProvider>();
        currentUserProviderMock.GetCurrentUserOid().Returns(CurrentUserOid);
        _currentUserProviderMockObject = currentUserProviderMock;

        var eventDispatcherMock = Substitute.For<IEventDispatcher>();
        _eventDispatcherMockObject = eventDispatcherMock;

        _dbContextOptions = new DbContextOptionsBuilder<CompletionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
            
        // ensure current user exists in db. Will be used when setting createdby / modifiedby
        if (context.Persons.SingleOrDefault(p => p.Guid == CurrentUserOid) is null)
        {
            _currentPerson = new Person(CurrentUserOid, "Ole", "Lukkøye", "ol", "ol@pcs.pcs");
            AddPerson(context, _currentPerson);
        }

        _projectA = new(TestPlantA, Guid.NewGuid(), "ProA", "ProA desc");
        _projectB = new(TestPlantA, Guid.NewGuid(), "ProB", "ProB desc");
        _closedProjectC = new(TestPlantA, Guid.NewGuid(), "ProC", "ProC desc") {IsClosed = true};

        AddProject(context, _projectA);
        AddProject(context, _projectB);
        AddProject(context, _closedProjectC);

        _raisedByOrg = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(), 
            "COM",
            "COM desc",
            LibraryType.COMPLETION_ORGANIZATION);
        _clearingByOrg = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(),
            "ENG",
            "ENG desc",
            LibraryType.COMPLETION_ORGANIZATION);
        _priority = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(),
            "P1",
            "P1 desc",
            LibraryType.PUNCHLIST_PRIORITY);
        _sorting = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(),
            "A",
            "A desc",
            LibraryType.PUNCHLIST_SORTING);
        _type = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(),
            "Paint",
            "Paint desc",
            LibraryType.PUNCHLIST_TYPE);

        AddLibraryItem(context, _raisedByOrg);
        AddLibraryItem(context, _clearingByOrg);
        AddLibraryItem(context, _priority);
        AddLibraryItem(context, _sorting);
        AddLibraryItem(context, _type);

        SetupNewDatabase(_dbContextOptions);
    }

    protected abstract void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions);

    protected Project GetProjectById(int projectId)
    {
        using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
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

    protected LibraryItem AddLibraryItem(CompletionContext context, LibraryItem libraryItem)
    {
        context.Library.Add(libraryItem);
        context.SaveChangesAsync().Wait();
        return libraryItem;
    }
}
