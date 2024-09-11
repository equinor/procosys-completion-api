using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport;

public static class FetchKeysCreator
{
    public static List<LibraryItemByPlant> CreateLibraryItemKeys(
        PunchItemImportMessage message)
    {
        var libraryItemKeys =
            new List<LibraryItemByPlant?>
                {
                    CreateLibraryItemKey(message.PunchListType, LibraryType.PUNCHLIST_TYPE),
                    CreateLibraryItemKey(message.ClearedByOrganization, LibraryType.COMPLETION_ORGANIZATION),
                    CreateLibraryItemKey(message.RaisedByOrganization, LibraryType.COMPLETION_ORGANIZATION)
                }
                .Where(x => x is not null)
                .Select(x => x!.Value)
                .Distinct().ToList();
        
        return libraryItemKeys;
    }

    public static ProjectByPlantKey CreateProjectKeys(
        PunchItemImportMessage importMessage)
        => CreateProjectKey(importMessage);
    

    private static LibraryItemByPlant? CreateLibraryItemKey(Optional<string?> libraryCode,
        LibraryType type)
    {
        if (libraryCode is not { HasValue: true, Value: not null })
        {
            return null;
        }
        var key = new LibraryItemByPlant(libraryCode.Value, type);
        return key;
    }
    
    private static ProjectByPlantKey CreateProjectKey(PunchItemImportMessage message)
        => new(message.ProjectName, message.Plant);
}
