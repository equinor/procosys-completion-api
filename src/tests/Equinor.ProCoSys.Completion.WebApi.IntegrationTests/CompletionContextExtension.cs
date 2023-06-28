using System;
using System.Linq;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
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
            KnownTestData.ProjectGuidA,
            KnownTestData.ProjectNameA,
            KnownTestData.ProjectDescriptionA);
        var punchA = SeedPunch(dbContext, plant, project, KnownTestData.PunchA);
        knownTestData.PunchAGuid = punchA.Guid;

        project = SeedProject(
            dbContext, 
            plant, 
            KnownTestData.ProjectGuidB,
            KnownTestData.ProjectNameB, 
            KnownTestData.ProjectDescriptionB);
        var punchB = SeedPunch(dbContext, plant, project, KnownTestData.PunchB);
        knownTestData.PunchBGuid = punchB.Guid;

        var link = SeedLink(dbContext, "Punch", punchA.Guid, "VG", "www.vg.no");
        knownTestData.LinkInPunchAGuid = link.Guid;

        var comment = SeedComment(dbContext, "Punch", punchA.Guid, "Comment");
        knownTestData.CommentInPunchAGuid = comment.Guid;

        var attachment = SeedAttachment(dbContext, plant, "Punch", punchA.Guid, "fil.txt");
        knownTestData.AttachmentInPunchAGuid = attachment.Guid;
    }

    private static void EnsureCurrentUserIsSeeded(CompletionContext dbContext, ICurrentUserProvider userProvider)
    {
        var personRepository = new PersonRepository(dbContext);
        var seeder = personRepository.GetByGuidAsync(userProvider.GetCurrentUserOid()).Result;
        if (seeder is null)
        {
            SeedCurrentUserAsPerson(dbContext, userProvider);
        }
    }

    private static void SeedCurrentUserAsPerson(CompletionContext dbContext, ICurrentUserProvider userProvider)
    {
        var personRepository = new PersonRepository(dbContext);
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

    private static Punch SeedPunch(CompletionContext dbContext, string plant, Project project, string title)
    {
        var punchRepository = new PunchRepository(dbContext);
        var punch = new Punch(plant, project, title);
        punchRepository.Add(punch);
        dbContext.SaveChangesAsync().Wait();
        return punch;
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
}
