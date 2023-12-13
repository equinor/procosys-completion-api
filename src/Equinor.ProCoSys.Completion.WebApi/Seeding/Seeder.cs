using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Equinor.ProCoSys.Completion.WebApi.Seeding;

public class Seeder : IHostedService
{
    private readonly string _testPlant = "PCS$OSEBERG_C";
    private readonly string _testProject = "959256";
    private readonly Guid _testProjectGuid = new ("eb38367c-37de-dd39-e053-2810000a174a");

    private static readonly Person s_seederUser = new (new Guid("12345678-1234-1234-1234-123456789123"), "Angus", "MacGyver", "am", "am@pcs.pcs");
    private readonly IServiceScopeFactory _serviceProvider;

    public Seeder(IServiceScopeFactory serviceProvider) => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var plantProvider = new SeedingPlantProvider("PCS$SEED");

        var userProvider = new SeederUserProvider();
        await using var dbContext = new CompletionContext(
            scope.ServiceProvider.GetRequiredService<DbContextOptions<CompletionContext>>(),
            plantProvider,
            scope.ServiceProvider.GetRequiredService<IEventDispatcher>(),
            userProvider);
        
        // If the seeder user exists in the database, it's already been seeded. Don't seed again.
        if (await dbContext.Persons.AnyAsync(p => p.Guid == s_seederUser.Guid, cancellationToken: cancellationToken))
        {
            return;
        }

        // Add the initial seeder user. Don't do this through the UnitOfWork as this expects/requires the current user to exist in the database.
        dbContext.Persons.Add(s_seederUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        var personRepository = new PersonRepository(dbContext, userProvider);
        personRepository.AddUsers(250);

        var projectRepository = new ProjectRepository(dbContext);
        projectRepository.Add(new Project(_testPlant, _testProjectGuid, _testProject, _testProject));
        projectRepository.AddProjects(_testPlant, 50);

        var libraryRepository = new LibraryItemRepository(dbContext);
        libraryRepository.Add(new LibraryItem(
            _testPlant,
            Guid.NewGuid(),
            "COM",
            "Commissioning",
            LibraryType.COMPLETION_ORGANIZATION));
        libraryRepository.AddLibraryItems(_testPlant, LibraryType.COMPLETION_ORGANIZATION, 50);

        libraryRepository.Add(new LibraryItem(
            _testPlant,
            Guid.NewGuid(),
            "01",
            "Low priority",
            LibraryType.PUNCHLIST_PRIORITY));
        libraryRepository.AddLibraryItems(_testPlant, LibraryType.PUNCHLIST_PRIORITY, 50);

        libraryRepository.Add(new LibraryItem(
            _testPlant,
            Guid.NewGuid(),
            "01",
            "Damage",
            LibraryType.PUNCHLIST_TYPE));
        libraryRepository.AddLibraryItems(_testPlant, LibraryType.PUNCHLIST_TYPE, 50);

        libraryRepository.Add(new LibraryItem(
            _testPlant,
            Guid.NewGuid(),
            "01",
            "01",
            LibraryType.PUNCHLIST_SORTING));
        libraryRepository.AddLibraryItems(_testPlant, LibraryType.PUNCHLIST_SORTING, 50);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private class SeederUserProvider : ICurrentUserProvider
    {
        public Guid GetCurrentUserOid() => s_seederUser.Guid;
        public bool HasCurrentUser => throw new NotImplementedException();
    }
}
