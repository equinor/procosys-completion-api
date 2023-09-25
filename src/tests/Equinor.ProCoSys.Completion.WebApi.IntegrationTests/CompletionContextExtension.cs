using System;
using System.Linq;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public static class CompletionContextExtension
{
    private static string _seederOid = "00000000-0000-0000-0000-999999999999";

    public static void CreateNewDatabaseWithCorrectSchema(this CompletionContext dbContext)
    {
        var migrations = dbContext.Database.GetPendingMigrations();
        if (migrations.Any())
        {
            dbContext.Database.Migrate();
        }
    }

    public static void Seed(this CompletionContext dbContext, IServiceProvider serviceProvider, KnownTestData knownTestData)
    {
        var userProvider = serviceProvider.GetRequiredService<CurrentUserProvider>();
        var plantProvider = serviceProvider.GetRequiredService<PlantProvider>();
        userProvider.SetCurrentUserOid(new Guid(_seederOid));
        plantProvider.SetPlant(knownTestData.Plant);
            
        /* 
         * Add the initial seeder user. Don't do this through the UnitOfWork as this expects/requires the current user to exist in the database.
         * This is the first user that is added to the database and will not get "Created" and "CreatedBy" data.
         */
        EnsureCurrentUserIsSeeded(dbContext, userProvider);

        var plant = plantProvider.Plant;
            
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
    }

    private static void EnsureCurrentUserIsSeeded(CompletionContext dbContext, ICurrentUserProvider userProvider)
    {
        var personRepository = new PersonRepository(dbContext, userProvider);
        var seeder = personRepository.GetByGuidAsync(userProvider.GetCurrentUserOid()).Result;
        if (seeder is null)
        {
            SeedCurrentUserAsPerson(dbContext, userProvider);
        }
    }

    private static void SeedCurrentUserAsPerson(CompletionContext dbContext, ICurrentUserProvider userProvider)
    {
        var personRepository = new PersonRepository(dbContext, userProvider);
        var person = new Person(userProvider.GetCurrentUserOid(), "Siri", "Seed", "SSEED", "siri@pcs.com");
        personRepository.Add(person);
        dbContext.SaveChangesAsync().Wait();
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
        dbContext.SaveChangesAsync().Wait();
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
        dbContext.SaveChangesAsync().Wait();
        return punchItem;
    }

    private static Link SeedLink(CompletionContext dbContext, string sourceType, Guid sourceGuid, string title, string url)
    {
        var linkRepository = new LinkRepository(dbContext);
        var link = new Link(sourceType, sourceGuid, title, url);
        linkRepository.Add(link);
        dbContext.SaveChangesAsync().Wait();
        return link;
    }

    private static Comment SeedComment(CompletionContext dbContext, string sourceType, Guid sourceGuid, string text)
    {
        var commentRepository = new CommentRepository(dbContext);
        var comment = new Comment(sourceType, sourceGuid, text);
        commentRepository.Add(comment);
        dbContext.SaveChangesAsync().Wait();
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
        dbContext.SaveChangesAsync().Wait();
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
        dbContext.SaveChangesAsync().Wait();
        return libraryItem;
    }
}
