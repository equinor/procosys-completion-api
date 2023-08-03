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
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.Test.Common;

public abstract class ReadOnlyTestsBase : TestsBase
{
    protected readonly string ProjectNameA = "ProA";
    protected readonly string ProjectNameB = "ProB";
    protected readonly string ProjectNameC = "ProC";
    protected readonly string RaisedByOrgCode = "COM";
    protected readonly string ClearingByOrgCode = "ENG";
    protected static readonly Guid ProjectGuidA = Guid.NewGuid();
    protected static readonly Guid ProjectGuidB = Guid.NewGuid();
    protected static readonly Guid ProjectGuidC = Guid.NewGuid();
    protected static readonly Guid RaisedByOrgGuid = Guid.NewGuid();
    protected static readonly Guid ClearingByOrgGuid = Guid.NewGuid();
    protected Project _projectA;
    protected Project _projectB;
    protected Project _closedProjectC;
    protected Person _currentPerson;
    protected LibraryItem _raisedByOrg;
    protected LibraryItem _clearingByOrg;
    protected readonly Guid CurrentUserOid = new ("12345678-1234-1234-1234-123456789123");
    protected DbContextOptions<CompletionContext> _dbContextOptions;
    protected IPlantProvider _plantProviderMockObject;
    protected ICurrentUserProvider _currentUserProviderMockObject;
    protected IEventDispatcher _eventDispatcherMockObject;

    [TestInitialize]
    public void SetupBase()
    {
        _plantProviderMockObject = _plantProviderMock.Object;

        var currentUserProviderMock = new Mock<ICurrentUserProvider>();
        currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Returns(CurrentUserOid);
        _currentUserProviderMockObject = currentUserProviderMock.Object;

        var eventDispatcherMock = new Mock<IEventDispatcher>();
        _eventDispatcherMockObject = eventDispatcherMock.Object;

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

        _projectA = new(TestPlantA, ProjectGuidA, ProjectNameA, $"{ProjectNameA} desc");
        _projectB = new(TestPlantA, ProjectGuidB, ProjectNameB, $"{ProjectNameB} desc");
        _closedProjectC = new(TestPlantA, ProjectGuidC, ProjectNameC, $"{ProjectNameC} desc") {IsClosed = true};

        AddProject(context, _projectA);
        AddProject(context, _projectB);
        AddProject(context, _closedProjectC);

        _raisedByOrg = new LibraryItem(
            TestPlantA,
            RaisedByOrgGuid, 
            RaisedByOrgCode,
            $"{RaisedByOrgCode} desc",
            LibraryTypes.COMPLETION_ORGANIZATION.ToString());
        _clearingByOrg = new LibraryItem(
            TestPlantA,
            ClearingByOrgGuid,
            ClearingByOrgCode,
            $"{ClearingByOrgCode} desc",
            LibraryTypes.COMPLETION_ORGANIZATION.ToString());

        AddLibraryItem(context, _raisedByOrg);
        AddLibraryItem(context, _clearingByOrg);

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
