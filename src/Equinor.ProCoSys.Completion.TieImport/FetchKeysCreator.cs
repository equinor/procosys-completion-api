using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.TieImport;

public static class FetchKeysCreator
{
    public static IEnumerable<TagNoByProjectNameAndPlantKey> CreateTagKeys(
        IReadOnlyCollection<PunchItemImportMessage> importMessages) =>
        importMessages.Select(message =>
            CreateTagKey(message.TagNo, message.ProjectName, message.Plant, message.FormType));

    public static ILookup<string, LibraryItemByPlant> CreateLibraryItemKeys(
        IReadOnlyCollection<PunchItemImportMessage> importMessages)
    {
        var libraryItemKeys = importMessages
            .SelectMany(message =>
                new List<LibraryItemByPlant?>
                {
                    CreateLibraryItemKey(message.PunchListType, message.Plant, LibraryType.PUNCHLIST_TYPE),
                    CreateLibraryItemKey(message.ClearedByOrganization, message.Plant,
                        LibraryType.COMPLETION_ORGANIZATION),
                    CreateLibraryItemKey(message.RaisedByOrganization, message.Plant,
                        LibraryType.COMPLETION_ORGANIZATION)
                }
            )
            .Where(x => x is not null)
            .Select(x => x!.Value)
            .Distinct()
            .ToLookup(k => k.Plant);

        return libraryItemKeys;
    }

    public static ILookup<string, PersonKey> CreatePersonKeys(
        IReadOnlyCollection<PunchItemImportMessage> importMessages)
    {
        var personKeys = importMessages
            .SelectMany(message =>
                new List<PersonKey?>
                {
                    CreatePersonKey(message.ClearedBy),
                    CreatePersonKey(message.VerifiedBy),
                    CreatePersonKey(message.RejectedBy)
                }
            )
            .Where(x => x is not null)
            .Select(x => x!.Value)
            .Distinct()
            .ToLookup(k => k.Email);

        return personKeys;
    }

    public static IEnumerable<ProjectByPlantKey> CreateProjectKeys(
        IReadOnlyCollection<PunchItemImportMessage> importMessages)
        => importMessages.Select(CreateProjectKey);

    private static TagNoByProjectNameAndPlantKey CreateTagKey(string tagNo, string projectName, string plant,
        string formType) =>
        new(tagNo, projectName, plant, formType);

    private static LibraryItemByPlant? CreateLibraryItemKey(Optional<string?> libraryCode, string plant,
        LibraryType type)
    {
        if (libraryCode is not { HasValue: true, Value: not null })
        {
            return null;
        }

        var key = new LibraryItemByPlant(libraryCode.Value, type, plant);

        return key;
    }

    private static PersonKey? CreatePersonKey(Optional<string?> personEmail)
    {
        if (personEmail is not { HasValue: true, Value: not null })
        {
            return null;
        }

        return new PersonKey(personEmail.Value);
    }

    private static ProjectByPlantKey CreateProjectKey(PunchItemImportMessage message)
        => new(message.ProjectName, message.Plant);
}
