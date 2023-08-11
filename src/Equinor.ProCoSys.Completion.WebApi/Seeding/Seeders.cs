using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Seeding;

public static class Seeders
{
    public static void AddUsers(this IPersonRepository personRepository, int entryCount)
    {
        for (var i = 0; i < entryCount; i++)
        {
            var user = new Person(Guid.NewGuid(), $"Firstname-{i}", $"Lastname-{i}", $"username-{i}", $"username-{i}@pcs.pcs");
            personRepository.Add(user);
        }
    }
    
    public static void AddProjects(this IProjectRepository projectRepository, string plant, int entryCount)
    {
        for (var i = 0; i < entryCount; i++)
        {
            var project = new Project(plant, Guid.NewGuid(), $"Name-{i}", $"Description-{i}");
            projectRepository.Add(project);
        }
    }

    public static void AddLibraryItems(this ILibraryItemRepository libraryItemRepository, string plant, LibraryType type, int entryCount)
    {
        for (var i = 0; i < entryCount; i++)
        {
            var project = new LibraryItem(plant, Guid.NewGuid(), $"Code-{i}", $"Description-{i}", type);
            libraryItemRepository.Add(project);
        }
    }
}
