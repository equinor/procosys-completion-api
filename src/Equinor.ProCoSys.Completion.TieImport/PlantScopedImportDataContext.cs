using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class PlantScopedImportDataContext(string plant)
{
    public string Plant { get; } = plant;
    public List<LibraryItem> Library { get; } = [];
    public List<Person> Persons { get; } = [];
    public List<Project> Projects { get; } = [];
    public List<TagCheckList> CheckLists { get; } = [];

    public void AddLibraryItems(IReadOnlyCollection<LibraryItem> libraryItems) => Library.AddRange(libraryItems);
    public void AddPersons(IReadOnlyCollection<Person> persons) => Persons.AddRange(persons);
    public void AddProjects(IReadOnlyCollection<Project> projects) => Projects.AddRange(projects);
    public void AddCheckList(IReadOnlyCollection<TagCheckList> checkLists) => CheckLists.AddRange(checkLists);

    public PunchItemImportReferences GetPunchItemImportMessageReferences(PunchItemImportMessage message)
    {
        var project = Projects.First(x => x.Name == message.ProjectName);
        var checkList = CheckLists.First(x => x.FormularType == message.FormType && x.TagNo == message.TagNo);
        var raisedByOrg = Library.First(x =>
            x.Code == message.RaisedByOrganization.Value && x.Type == LibraryType.COMPLETION_ORGANIZATION);
        var clearingByOrg = Library.First(x =>
            x.Code == message.ClearedByOrganization.Value && x.Type == LibraryType.COMPLETION_ORGANIZATION);
        var type = Library.FirstOrDefault(x =>
            x.Code == message.PunchListType.Value && x.Type == LibraryType.PUNCHLIST_TYPE);

        return new PunchItemImportReferences(
            project.Guid,
            checkList.ProCoSysGuid,
            raisedByOrg.Guid,
            clearingByOrg.Guid,
            type?.Guid
        );
    }
}
