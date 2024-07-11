using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class PlantScopedImportDataContext
{
    public List<LibraryItem> Library { get; } = new();
    public List<Person> Persons { get; } = new();

    public void AddLibraryItems(LibraryItem[] libraryItems) => Library.AddRange(libraryItems);
    public void AddPersons(Person[] persons) => Persons.AddRange(persons);
}
