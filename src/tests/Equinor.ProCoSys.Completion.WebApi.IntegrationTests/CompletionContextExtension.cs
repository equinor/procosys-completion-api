using System;
using System.Linq;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public static class CompletionContextExtension
{
    private const string SeederOid = "00000000-0000-0000-0000-999999999999";

    public static void CreateNewDatabaseWithCorrectSchema(this CompletionContext dbContext)
    {
        var migrations = dbContext.Database.GetPendingMigrations();
        if (migrations.Any())
        {
            dbContext.Database.Migrate();
        }
    }

    /* 
     * Add the initial seeder user. Don't do this through the UnitOfWork as this expects/requires the current user to exist in the database.
     * This is the first user that is added to the database and will not get "Created" and "CreatedBy" data.
     */
    public static void SeedCurrentUser(this CompletionContext dbContext)
        => SeedPerson(dbContext, SeederOid, "Siri", "Seed", "SSEED", "siri@pcs.com", false);

    public static void SeedPersonData(this CompletionContext dbContext, TestProfile profile)
        => SeedPerson(
            dbContext,
            profile.Oid,
            profile.FirstName,
            profile.LastName,
            profile.UserName,
            profile.Email,
            profile.Superuser);

    public static void SeedLabels(this CompletionContext dbContext)
    {
        var labelA = new Label(KnownData.LabelA);
        var labelB = new Label(KnownData.LabelB);
        var labelReject = new Label(KnownData.LabelReject);
        
        var labelRepository = new LabelRepository(dbContext);
        labelRepository.Add(labelA);
        labelRepository.Add(labelB);
        labelRepository.Add(labelReject);

        var labelEntity = new LabelEntity(KnownData.EntityTypeWithLabels);
        labelA.MakeLabelAvailableFor(labelEntity);
        labelB.MakeLabelAvailableFor(labelEntity);

        var labelEntityRepository = new LabelEntityRepository(dbContext);
        labelEntityRepository.Add(labelEntity);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    public static void SeedMailTemplates(this CompletionContext dbContext)
    {
        var mailTemplateA = new MailTemplate(KnownData.MailTemplateA, Guid.NewGuid().ToString(), $"<p>{Guid.NewGuid()}</p>");
        var mailTemplateB = new MailTemplate(KnownData.MailTemplateB, Guid.NewGuid().ToString(), $"<p>{Guid.NewGuid()}</p>");

        var mailTemplateRepository = new MailTemplateRepository(dbContext);
        mailTemplateRepository.Add(mailTemplateA);
        mailTemplateRepository.Add(mailTemplateB);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    public static void SeedPlantData(this CompletionContext dbContext, IServiceProvider serviceProvider, KnownTestData knownTestData)
    {
        var userProvider = serviceProvider.GetRequiredService<CurrentUserProvider>();
        userProvider.SetCurrentUserOid(new Guid(SeederOid));
        var plant = knownTestData.Plant;
            
        var project = SeedProject(
            dbContext,
            plant,
            KnownData.ProjectGuidA[plant],
            "ProjectNameA",
            "ProjectDescriptionA");

        var raisedByOrg = SeedLibrary(
            dbContext,
            plant,
            KnownData.RaisedByOrgGuid[plant],
            "COM",
            LibraryType.COMPLETION_ORGANIZATION);

        var clearingByOrg = SeedLibrary(
            dbContext,
            plant,
            KnownData.ClearingByOrgGuid[plant],
            "ENG",
            LibraryType.COMPLETION_ORGANIZATION);

        var priority = SeedLibrary(
            dbContext,
            plant,
            KnownData.PriorityGuid[plant],
            "P1",
            LibraryType.COMM_PRIORITY);
        ClassifyPriorityAsPunchPriority(dbContext, priority);
        knownTestData.PunchLibraryItemGuids.Add(priority.Guid);

        var sorting = SeedLibrary(
            dbContext,
            plant,
            KnownData.SortingGuid[plant],
            "A",
            LibraryType.PUNCHLIST_SORTING);
        knownTestData.PunchLibraryItemGuids.Add(sorting.Guid);

        var type = SeedLibrary(
            dbContext,
            plant,
            KnownData.TypeGuid[plant],
            "Painting",
            LibraryType.PUNCHLIST_TYPE);

        var punchItem = SeedPunchItem(
            dbContext,
            plant,
            project,
            KnownData.CheckListGuidA[plant],
            Category.PA,
            "PunchItemA",
            raisedByOrg,
            clearingByOrg,
            priority,
            sorting,
            type);
        knownTestData.PunchItem = punchItem;

        project = SeedProject(
            dbContext, 
            plant,
            KnownData.ProjectGuidB[plant],
            "ProjectNameB", 
            "ProjectDescriptionB");
        SeedPunchItem(
            dbContext,
            plant,
            project,
            KnownData.CheckListGuidA[plant],
            Category.PA,
            "PunchItemB",
            raisedByOrg,
            clearingByOrg);

        var link = SeedLink(dbContext, nameof(PunchItem), punchItem.Guid, "VG", "www.vg.no");
        knownTestData.LinkInPunchItemAGuid = link.Guid;

        var comment = SeedComment(dbContext, nameof(PunchItem), punchItem.Guid, "Comment");
        knownTestData.CommentInPunchItemAGuid = comment.Guid;

        var historyItem = SeedHistory(
            dbContext, 
            punchItem.Guid, "History display name", 
            userProvider.GetCurrentUserOid(), 
            "History full name", 
            DateTime.UtcNow);

        knownTestData.HistoryInPunchItem = historyItem;

        var attachment = SeedAttachment(dbContext, project.Name, nameof(PunchItem), punchItem.Guid, "fil.txt");
        knownTestData.AttachmentInPunchItemAGuid = attachment.Guid;

        SeedWorkOrder(
            dbContext,
            plant,
            KnownData.OriginalWorkOrderGuid[plant],
            KnownData.WorkOrderNo[plant]);
        SeedWorkOrder(
            dbContext,
            plant,
            KnownData.WorkOrderGuid[plant],
            KnownData.OriginalWorkOrderNo[plant]);

        SeedSWCR(
            dbContext,
            plant,
            KnownData.SWCRGuid[plant],
            KnownData.SWCRNo[plant]);

        SeedDocument(
            dbContext,
            plant,
            KnownData.DocumentGuid[plant],
            KnownData.DocumentNo[plant]);
    }

    private static void ClassifyPriorityAsPunchPriority(CompletionContext dbContext, LibraryItem priority)
    {
        priority.AddClassification(new Classification(Guid.NewGuid(), Classification.PunchPriority));
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedWorkOrder(CompletionContext dbContext, string plant, Guid guid, string no)
    {
        var workOrder = new WorkOrder(plant, guid, no)
        {
            ProCoSys4LastUpdated = DateTime.Now, SyncTimestamp = DateTime.Now, IsVoided = false
        };
        var workOrderRepository = new WorkOrderRepository(dbContext);
        workOrderRepository.Add(workOrder);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedSWCR(CompletionContext dbContext, string plant, Guid guid, int no)
    {
        var swcr = new SWCR(plant, guid, no)  {
            ProCoSys4LastUpdated = DateTime.Now, SyncTimestamp = DateTime.Now, IsVoided = false
        };
        var swcrRepository = new SWCRRepository(dbContext);
        swcrRepository.Add(swcr);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedDocument(CompletionContext dbContext, string plant, Guid guid, string no)
    {
        var document = new Document(plant, guid, no) {
            ProCoSys4LastUpdated = DateTime.Now, SyncTimestamp = DateTime.Now, IsVoided = false
        };
        var documentRepository = new DocumentRepository(dbContext);
        documentRepository.Add(document);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedPerson(
        CompletionContext dbContext,
        string oid,
        string firstName,
        string lastName,
        string userName,
        string email,
        bool superuser)
    {
        var person = new Person(new Guid(oid), firstName, lastName, userName, email, superuser) {
            ProCoSys4LastUpdated = DateTime.Now, SyncTimestamp = DateTime.Now
        };
        var personRepository = new PersonRepository(dbContext, null!, null!);
        personRepository.Add(person);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static Project SeedProject(
        CompletionContext dbContext,
        string plant,
        Guid guid,
        string name,
        string desc)
    {
        var projectRepository = new ProjectRepository(dbContext);
        var project = new Project(plant, guid, name, desc) {
            ProCoSys4LastUpdated = DateTime.Now, SyncTimestamp = DateTime.Now, IsVoided = false
        };
        projectRepository.Add(project);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return project;
    }

    private static PunchItem SeedPunchItem(
        CompletionContext dbContext,
        string plant,
        Project project,
        Guid checkListGuid,
        Category category,
        string description,
        LibraryItem raisedByOrg,
        LibraryItem clearingByOrg,
        LibraryItem priority = null,
        LibraryItem sorting = null,
        LibraryItem type = null)
    {
        var punchItemRepository = new PunchItemRepository(dbContext);
        var punchItem = new PunchItem(plant, project, checkListGuid, category, description, raisedByOrg, clearingByOrg);
        if (priority is not null)
        {
            punchItem.SetPriority(priority);
        }
        if (sorting is not null)
        {
            punchItem.SetSorting(sorting);
        }
        if (type is not null)
        {
            punchItem.SetType(type);
        }
        punchItemRepository.Add(punchItem);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return punchItem;
    }

    private static Link SeedLink(
        CompletionContext dbContext, 
        string parentType, 
        Guid parentGuid, 
        string title, 
        string url)
    {
        var linkRepository = new LinkRepository(dbContext);
        var link = new Link(parentType, parentGuid, title, url);
        linkRepository.Add(link);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return link;
    }

    private static Comment SeedComment(CompletionContext dbContext, string parentType, Guid parentGuid, string text)
    {
        var commentRepository = new CommentRepository(dbContext);
        var comment = new Comment(parentType, parentGuid, text);
        commentRepository.Add(comment);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return comment;
    }

    private static HistoryItem SeedHistory(
        CompletionContext dbContext, 
        Guid parentGuid, 
        string displayName, 
        Guid oid, 
        string fullName, 
        DateTime utc)
    {
        var historyItemRepository = new HistoryItemRepository(dbContext);
        var historyItem = new HistoryItem(parentGuid, displayName, oid, fullName, utc);
        historyItem.AddProperty(new Property("Property name", "Value display type"));
        historyItemRepository.Add(historyItem);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return historyItem;
    }

    private static Attachment SeedAttachment(
        CompletionContext dbContext,
        string project,
        string parentType,
        Guid parentGuid,
        string fileName)
    {
        var attachmentRepository = new AttachmentRepository(dbContext);
        var attachment = new Attachment(project, parentType, parentGuid, fileName);
        attachmentRepository.Add(attachment);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return attachment;
    }

    private static LibraryItem SeedLibrary(
        CompletionContext dbContext,
        string plant,
        Guid guid,
        string code,
        LibraryType type)
    {
        var libraryItemRepository = new LibraryItemRepository(dbContext);
        var libraryItem = new LibraryItem(plant, guid, code, $"{code} desc", type);
        libraryItemRepository.Add(libraryItem);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return libraryItem;
    }
}
