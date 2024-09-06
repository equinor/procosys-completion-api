using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.TieImport;

public static class FetchKeysCreator
{
    public static ILookup<string, LibraryItemByPlant> CreateLibraryItemKeys(
        PunchItemImportMessage message)
    {
        var libraryItemKeys =
                new List<LibraryItemByPlant?>
                {
                    CreateLibraryItemKey(message.PunchListType, message.Plant, LibraryType.PUNCHLIST_TYPE),
                    CreateLibraryItemKey(message.ClearedByOrganization, message.Plant,
                        LibraryType.COMPLETION_ORGANIZATION),
                    CreateLibraryItemKey(message.RaisedByOrganization, message.Plant,
                        LibraryType.COMPLETION_ORGANIZATION)
                }
                .Where(x => x is not null).Select(x => x!.Value)
                .Distinct()
                .ToLookup(k => k.Plant);

        return libraryItemKeys;
    }

    public static ProjectByPlantKey CreateProjectKeys(
        PunchItemImportMessage importMessage)
        => CreateProjectKey(importMessage);
    

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
    
    private static ProjectByPlantKey CreateProjectKey(PunchItemImportMessage message)
        => new(message.ProjectName, message.Plant);
}
