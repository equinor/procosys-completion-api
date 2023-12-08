using System;
using System.Linq;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Test.Common;

public abstract class ReadOnlyTestsBase : TestsBase
{
    protected int _projectAId;
    protected int _projectBId;
    protected int _closedProjectCId;
    protected int _raisedByOrgId;
    protected int _clearingByOrgId;
    protected int _priorityId;
    protected int _sortingId;
    protected int _typeId;
    protected int _documentId;
    protected int _swcrId;
    protected int _workOrderId;
    protected readonly Guid CurrentUserOid = new ("12345678-1234-1234-1234-123456789123");
    protected readonly string LabelTextA = "A";
    protected readonly string LabelTextB = "B";
    protected readonly string LabelTextC = "C";
    protected readonly string LabelTextVoided = "V";
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
            AddPerson(context, new Person(CurrentUserOid, "Ole", "Lukkøye", "ol", "ol@pcs.pcs"));
        }

        var projectA = new Project(TestPlantA, Guid.NewGuid(), "ProA", "ProA desc");
        var projectB = new Project(TestPlantA, Guid.NewGuid(), "ProB", "ProB desc");
        var closedProjectC = new Project(TestPlantA, Guid.NewGuid(), "ProC", "ProC desc") {IsClosed = true};

        _projectAId = AddProject(context, projectA);
        _projectBId = AddProject(context, projectB);
        _closedProjectCId = AddProject(context, closedProjectC);

        var raisedByOrg = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(), 
            "COM",
            "COM desc",
            LibraryType.COMPLETION_ORGANIZATION);
        var clearingByOrg = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(),
            "ENG",
            "ENG desc",
            LibraryType.COMPLETION_ORGANIZATION);
        var priority = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(),
            "P1",
            "P1 desc",
            LibraryType.PUNCHLIST_PRIORITY);
        var sorting = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(),
            "A",
            "A desc",
            LibraryType.PUNCHLIST_SORTING);
        var type = new LibraryItem(
            TestPlantA,
            Guid.NewGuid(),
            "Paint",
            "Paint desc",
            LibraryType.PUNCHLIST_TYPE);

        _raisedByOrgId = AddLibraryItem(context, raisedByOrg);
        _clearingByOrgId = AddLibraryItem(context, clearingByOrg);
        _priorityId = AddLibraryItem(context, priority);
        _sortingId = AddLibraryItem(context, sorting);
        _typeId = AddLibraryItem(context, type);

        var document = new Document(TestPlantA, Guid.NewGuid(), "1A");
        _documentId = AddDocument(context, document);

        var swcr = new SWCR(TestPlantA, Guid.NewGuid(), 1);
        _swcrId = AddSWCR(context, swcr);

        var workOrder = new WorkOrder(TestPlantA, Guid.NewGuid(), "004");
        _workOrderId = AddWorkOrder(context, workOrder);

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

    protected int AddProject(CompletionContext context, Project project)
    {
        context.Projects.Add(project);
        context.SaveChangesAsync().Wait();
        return project.Id;
    }

    protected int AddLibraryItem(CompletionContext context, LibraryItem libraryItem)
    {
        context.Library.Add(libraryItem);
        context.SaveChangesAsync().Wait();
        return libraryItem.Id;
    }

    private int AddWorkOrder(CompletionContext context, WorkOrder workOrder)
    {
        context.WorkOrders.Add(workOrder);
        context.SaveChangesAsync().Wait();
        return workOrder.Id;
    }

    private int AddSWCR(CompletionContext context, SWCR swcr)
    {
        context.SWCRs.Add(swcr);
        context.SaveChangesAsync().Wait();
        return swcr.Id;
    }

    private int AddDocument(CompletionContext context, Document document)
    {
        context.Documents.Add(document);
        context.SaveChangesAsync().Wait();
        return document.Id;
    }

    private void AddLabel(CompletionContext context, Label label)
    {
        context.Labels.Add(label);
        context.SaveChangesAsync().Wait();
    }

    public void Add4UnorderedLabelsInclusiveAVoidedLabel(CompletionContext context)
    {
        AddLabel(context, new Label(LabelTextA));
        AddLabel(context, new Label(LabelTextC));
        AddLabel(context, new Label(LabelTextVoided) { IsVoided = true });
        AddLabel(context, new Label(LabelTextB));
    }
}
