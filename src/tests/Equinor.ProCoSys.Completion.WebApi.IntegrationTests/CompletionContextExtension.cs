using System;
using System.Linq;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
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
        => SeedPerson(dbContext, SeederOid, "Siri", "Seed", "SSEED", "siri@pcs.com");

    public static void SeedPersonData(this CompletionContext dbContext, TestProfile profile)
        => SeedPerson(dbContext, profile.Oid, profile.FirstName, profile.LastName, profile.UserName, profile.Email);

    public static void SeedPlantData(this CompletionContext dbContext, IServiceProvider serviceProvider, KnownTestData knownTestData)
    {
        var userProvider = serviceProvider.GetRequiredService<CurrentUserProvider>();
        userProvider.SetCurrentUserOid(new Guid(SeederOid));
        var plant = knownTestData.Plant;
            
        var project = SeedProject(
            dbContext,
            plant,
            KnownPlantData.ProjectGuidA[plant],
            "ProjectNameA",
            "ProjectDescriptionA");

        var raisedByOrg = SeedLibrary(
            dbContext,
            plant,
            KnownPlantData.RaisedByOrgGuid[plant],
            "COM",
            LibraryType.COMPLETION_ORGANIZATION);

        var clearingByOrg = SeedLibrary(
            dbContext,
            plant,
            KnownPlantData.ClearingByOrgGuid[plant],
            "ENG",
            LibraryType.COMPLETION_ORGANIZATION);

        var priority = SeedLibrary(
            dbContext,
            plant,
            KnownPlantData.PriorityGuid[plant],
            "P1",
            LibraryType.PUNCHLIST_PRIORITY);

        var sorting = SeedLibrary(
            dbContext,
            plant,
            KnownPlantData.SortingGuid[plant],
            "A",
            LibraryType.PUNCHLIST_SORTING);

        var type = SeedLibrary(
            dbContext,
            plant,
            KnownPlantData.TypeGuid[plant],
            "Painting",
            LibraryType.PUNCHLIST_TYPE);

        var punchItem = SeedPunchItem(
            dbContext,
            plant,
            project,
            KnownPlantData.CheckListGuid[plant],
            Category.PA,
            "PunchItemA",
            raisedByOrg,
            clearingByOrg,
            priority,
            sorting,
            type);
        knownTestData.PunchItemGuid = punchItem.Guid;

        project = SeedProject(
            dbContext, 
            plant,
            KnownPlantData.ProjectGuidB[plant],
            "ProjectNameB", 
            "ProjectDescriptionB");
        SeedPunchItem(
            dbContext,
            plant,
            project,
            KnownPlantData.CheckListGuid[plant],
            Category.PA,
            "PunchItemB",
            raisedByOrg,
            clearingByOrg);

        var link = SeedLink(dbContext, nameof(PunchItem), punchItem.Guid, "VG", "www.vg.no");
        knownTestData.LinkInPunchItemAGuid = link.Guid;

        var comment = SeedComment(dbContext, nameof(PunchItem), punchItem.Guid, "Comment");
        knownTestData.CommentInPunchItemAGuid = comment.Guid;

        var attachment = SeedAttachment(dbContext, plant, nameof(PunchItem), punchItem.Guid, "fil.txt");
        knownTestData.AttachmentInPunchItemAGuid = attachment.Guid;

        SeedWorkOrder(
            dbContext,
            plant,
            KnownPlantData.OriginalWorkOrderGuid[plant]);
        SeedWorkOrder(
            dbContext,
            plant,
            KnownPlantData.WorkOrderGuid[plant]);

        var swcrNo = 10;
        SeedSWCR(
            dbContext,
            plant,
            KnownPlantData.SWCRGuid[plant],
            ++swcrNo);

        SeedDocument(
            dbContext,
            plant,
            KnownPlantData.DocumentGuid[plant]);
    }

    private static void SeedWorkOrder(CompletionContext dbContext, string plant, Guid guid)
    {
        var workOrder = new WorkOrder(plant, guid, Guid.NewGuid().ToString().Substring(0, WorkOrder.NoLengthMax));
        var workOrderRepository = new WorkOrderRepository(dbContext);
        workOrderRepository.Add(workOrder);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedSWCR(CompletionContext dbContext, string plant, Guid guid, int no)
    {
        var swcr = new SWCR(plant, guid, no);
        var swcrRepository = new SWCRRepository(dbContext);
        swcrRepository.Add(swcr);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedDocument(CompletionContext dbContext, string plant, Guid guid)
    {
        var document = new Document(plant, guid, Guid.NewGuid().ToString());
        var documentRepository = new DocumentRepository(dbContext);
        documentRepository.Add(document);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedPerson(CompletionContext dbContext, string oid, string firstName, string lastName, string userName, string email)
    {
        var person = new Person(new Guid(oid), firstName, lastName, userName, email);
        var personRepository = new PersonRepository(dbContext, null!);
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
        var project = new Project(plant, guid, name, desc);
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

    private static Link SeedLink(CompletionContext dbContext, string sourceType, Guid sourceGuid, string title, string url)
    {
        var linkRepository = new LinkRepository(dbContext);
        var link = new Link(sourceType, sourceGuid, title, url);
        linkRepository.Add(link);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return link;
    }

    private static Comment SeedComment(CompletionContext dbContext, string sourceType, Guid sourceGuid, string text)
    {
        var commentRepository = new CommentRepository(dbContext);
        var comment = new Comment(sourceType, sourceGuid, text);
        commentRepository.Add(comment);
        dbContext.SaveChangesAsync().GetAwaiter().GetResult();
        return comment;
    }

    private static Attachment SeedAttachment(
        CompletionContext dbContext,
        string plant,
        string sourceType,
        Guid sourceGuid,
        string fileName)
    {
        var attachmentRepository = new AttachmentRepository(dbContext);
        var attachment = new Attachment(sourceType, sourceGuid, plant, fileName);
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
