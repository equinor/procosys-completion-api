using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class PlantScopedImportDataContext(string plant)
{
    public string Plant { get; } = plant;
    public List<LibraryItem> Library { get; } = [];
    public List<Person> Persons { get; } = [];
    public List<Project> Projects { get; } = [];
    public List<TagCheckList> CheckLists { get; } = [];
    public List<PunchItem> PunchItems { get; } = [];

    public void AddLibraryItems(IReadOnlyCollection<LibraryItem> libraryItems) => Library.AddRange(libraryItems);
    public void AddPersons(IReadOnlyCollection<Person> persons) => Persons.AddRange(persons);
    public void AddProjects(IReadOnlyCollection<Project> projects) => Projects.AddRange(projects);
    public void AddCheckList(IReadOnlyCollection<TagCheckList> checkLists) => CheckLists.AddRange(checkLists);
    public void AddPunchItems(PunchItem[] punchItems) => PunchItems.AddRange(punchItems);
}
