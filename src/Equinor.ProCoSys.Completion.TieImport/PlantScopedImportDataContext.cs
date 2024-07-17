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

    public (PunchItemImportReferences References, IReadOnlyCollection<ImportError> Errors)
        GetPunchItemImportMessageReferences(PunchItemImportMessage message)
    {
        var errors = new List<ImportError>();
        var project = Projects.FirstOrDefault(x => x.Name == message.ProjectName);
        if (project is null)
        {
            errors.Add(message.ToImportError($"Project '{message.ProjectName}' not found"));
        }

        var checkList = CheckLists.FirstOrDefault(x => x.FormularType == message.FormType && x.TagNo == message.TagNo);
        if (checkList is null)
        {
            errors.Add(message.ToImportError($"CheckList '{message.FormType}' for Tag '{message.TagNo}' not found"));
        }

        var raisedByOrg = Library.FirstOrDefault(x =>
            x.Code == message.RaisedByOrganization.Value && x.Type == LibraryType.COMPLETION_ORGANIZATION);
        if (raisedByOrg is null)
        {
            errors.Add(message.ToImportError($"RaisedByOrganization '{message.RaisedByOrganization.Value}' not found"));
        }

        var clearingByOrg = Library.FirstOrDefault(x =>
            x.Code == message.ClearedByOrganization.Value && x.Type == LibraryType.COMPLETION_ORGANIZATION);
        if (clearingByOrg is null)
        {
            errors.Add(message.ToImportError(
                $"RaisedByOrganization '{message.ClearedByOrganization.Value}' not found"));
        }

        var type = Library.FirstOrDefault(x =>
            x.Code == message.PunchListType.Value && x.Type == LibraryType.PUNCHLIST_TYPE);

        return (new PunchItemImportReferences(
            project?.Guid ?? Guid.Empty,
            checkList?.ProCoSysGuid ?? Guid.Empty,
            raisedByOrg?.Guid ?? Guid.Empty,
            clearingByOrg?.Guid ?? Guid.Empty,
            type?.Guid
        ), errors);
    }

    public void AddPunchItems(PunchItem[] punchItems) => PunchItems.AddRange(punchItems);
}
