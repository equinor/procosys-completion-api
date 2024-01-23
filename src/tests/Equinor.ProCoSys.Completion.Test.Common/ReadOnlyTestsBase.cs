using System;
using System.Collections.Generic;
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
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Test.Common;

public abstract class ReadOnlyTestsBase : TestsBase
{
    protected Dictionary<string, int> _projectAId = new ();
    protected Dictionary<string, int> _projectBId = new ();
    protected Dictionary<string, int> _closedProjectCId = new ();
    protected Dictionary<string, int> _raisedByOrgId = new ();
    protected Dictionary<string, int> _clearingByOrgId = new ();
    protected Dictionary<string, int> _priorityId = new ();
    protected Dictionary<string, int> _sortingId = new ();
    protected Dictionary<string, int> _typeId = new ();
    protected Dictionary<string, int> _documentId = new ();
    protected Dictionary<string, int> _swcrId = new ();
    protected Dictionary<string, int> _workOrderId = new ();
    protected readonly Guid CurrentUserOid = new ("00000000-0000-0000-0000-000000000111");
    protected readonly Guid PersonAOid = new ("00000000-0000-0000-0000-000000000aaa");
    protected readonly Guid PersonBOid = new ("00000000-0000-0000-0000-000000000bbb");
    protected readonly string LabelTextA = "A";
    protected readonly string LabelTextB = "B";
    protected readonly string LabelTextC = "C";
    protected readonly string LabelTextVoided = "V";
    protected readonly string MailTemplateCodeA = "A";
    protected readonly string MailTemplateCodeB = "B";
    protected readonly string MailTemplateCodeC = "C";
    protected readonly string MailTemplateCodeVoided = "V";
    protected DbContextOptions<CompletionContext> _dbContextOptions;
    protected ICurrentUserProvider _currentUserProviderMock;
    protected IEventDispatcher _eventDispatcherMock;

    [TestInitialize]
    public void SetupBase()
    {
        _currentUserProviderMock = Substitute.For<ICurrentUserProvider>();
        _currentUserProviderMock.GetCurrentUserOid().Returns(CurrentUserOid);

        _eventDispatcherMock = Substitute.For<IEventDispatcher>();

        _dbContextOptions = new DbContextOptionsBuilder<CompletionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);
            
        // ensure current user exists in db. Will be used when setting createdby / modifiedby
        if (context.Persons.SingleOrDefault(p => p.Guid == CurrentUserOid) is null)
        {
            AddPerson(context, new Person(CurrentUserOid, "Ole", "Lukkøye", "ol", "ol@pcs.pcs", false));
        }

        AddPerson(context, new Person(PersonAOid, "A", "Ab", "aa", "aa@pcs.pcs", false));
        AddPerson(context, new Person(PersonBOid, "B", "Bb", "bb", "bb@pcs.pcs", false));

        AddTestDataToPlant(TestPlantA, context);
        AddTestDataToPlant(TestPlantB, context);

        SetupNewDatabase(_dbContextOptions);
    }

    private void AddTestDataToPlant(string plant, CompletionContext context)
    {
        var projectA = new Project(plant, Guid.NewGuid(), "ProA", "ProA desc");
        var projectB = new Project(plant, Guid.NewGuid(), "ProB", "ProB desc");
        var closedProjectC = new Project(plant, Guid.NewGuid(), "ProC", "ProC desc") { IsClosed = true };

        _projectAId.Add(plant, AddProject(context, projectA));
        _projectBId.Add(plant, AddProject(context, projectB));
        _closedProjectCId.Add(plant, AddProject(context, closedProjectC));

        var raisedByOrg = new LibraryItem(
            plant,
            Guid.NewGuid(),
            "COM",
            "COM desc",
            LibraryType.COMPLETION_ORGANIZATION);
        var clearingByOrg = new LibraryItem(
            plant,
            Guid.NewGuid(),
            "ENG",
            "ENG desc",
            LibraryType.COMPLETION_ORGANIZATION);
        var priority = new LibraryItem(
            plant,
            Guid.NewGuid(),
            "P1",
            "P1 desc",
            LibraryType.PUNCHLIST_PRIORITY);
        var sorting = new LibraryItem(
            plant,
            Guid.NewGuid(),
            "A",
            "A desc",
            LibraryType.PUNCHLIST_SORTING);
        var type = new LibraryItem(
            plant,
            Guid.NewGuid(),
            "Paint",
            "Paint desc",
            LibraryType.PUNCHLIST_TYPE);

        _raisedByOrgId.Add(plant, AddLibraryItem(context, raisedByOrg));
        _clearingByOrgId.Add(plant, AddLibraryItem(context, clearingByOrg));
        _priorityId.Add(plant, AddLibraryItem(context, priority));
        _sortingId.Add(plant, AddLibraryItem(context, sorting));
        _typeId.Add(plant, AddLibraryItem(context, type));

        var document = new Document(plant, Guid.NewGuid(), "1A");
        _documentId.Add(plant, AddDocument(context, document));

        var swcr = new SWCR(plant, Guid.NewGuid(), 1);
        _swcrId.Add(plant, AddSWCR(context, swcr));

        var workOrder = new WorkOrder(plant, Guid.NewGuid(), "004");
        _workOrderId.Add(plant, AddWorkOrder(context, workOrder));
    }

    protected abstract void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions);

    protected Project GetProjectById(int projectId)
    {
        using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);
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

    private void AddMailTemplate(CompletionContext context, MailTemplate mailTemplate)
    {
        context.MailTemplates.Add(mailTemplate);
        context.SaveChangesAsync().Wait();
    }

    public void Add4UnorderedGlobalMailTemplatesInclusiveAVoidedMailTemplate(CompletionContext context)
        => Add4UnorderedMailTemplatesInclusiveAVoidedMailTemplate(context, null);

    public void Add4UnorderedMailTemplatesForPlantInclusiveAVoidedMailTemplate(
        CompletionContext context,
        string plant)
        => Add4UnorderedMailTemplatesInclusiveAVoidedMailTemplate(context, plant);

    public void Add4UnorderedMailTemplatesInclusiveAVoidedMailTemplate(CompletionContext context, string plant)
    {
        AddMailTemplate(context, 
            new MailTemplate(MailTemplateCodeA, Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
            {
                Plant = plant
            });
        AddMailTemplate(context, 
            new MailTemplate(MailTemplateCodeC, Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
            {
                Plant = plant
            });
        AddMailTemplate(context,
            new MailTemplate(MailTemplateCodeVoided, Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
            {
                IsVoided = true,
                Plant = plant
            });
        AddMailTemplate(context, 
            new MailTemplate(MailTemplateCodeB, Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
            {
                Plant = plant
            });
    }
}
