using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class PlantScopedImportDataContext
{
    public List<LibraryItem> Library { get; } = new();
    public List<Person> Persons { get; } = new();
    public List<Project> Projects { get; } = new();
    public List<TagCheckList> CheckLists { get; } = new();

    public void AddLibraryItems(IReadOnlyCollection<LibraryItem> libraryItems) => Library.AddRange(libraryItems);
    public void AddPersons(IReadOnlyCollection<Person> persons) => Persons.AddRange(persons);
    public void AddProjects(IReadOnlyCollection<Project> projects) => Projects.AddRange(projects);
    public void AddCheckList(IReadOnlyCollection<TagCheckList> checkLists) => CheckLists.AddRange(checkLists);
}
