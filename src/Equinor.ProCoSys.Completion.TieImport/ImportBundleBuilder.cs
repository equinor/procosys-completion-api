using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class ImportBundleBuilder(IImportDataFetcher importDataFetcher)
{
    public async Task<ImportDataBundle> BuildAsync(
        PunchItemImportMessage importMessage, CancellationToken cancellationToken)
    {
        var libraryItemsByPlant = FetchKeysCreator.CreateLibraryItemKeys(importMessage);
        var projectByPlantKey = FetchKeysCreator.CreateProjectKeys(importMessage);
        
        var project = await FetchProjectsAsync(projectByPlantKey, cancellationToken);
        
        var bundle = new ImportDataBundle(importMessage.Plant, project);
        bundle.AddLibraryItems(await FetchLibraryItemsForPlantAsync(importMessage.Plant,
            libraryItemsByPlant,
            cancellationToken));
        
        // Fetch checklist Guid, then fetch the full checklist if found
        var checkListGuid = await FetchChecklistGuidAsync(importMessage, cancellationToken);
        
        if (checkListGuid.HasValue)
        {
            bundle.CheckList = await importDataFetcher.GetCheckListByGuidAsync(checkListGuid.Value, cancellationToken);
            
            if (bundle.CheckList is not null)
            {
                bundle.CheckListProject = await FetchProjectByGuidAsync(bundle.CheckList.ProjectGuid, importMessage.Plant, cancellationToken);
            }
        }
        
        bundle.AddPersons(await FetchImportUserAsync(cancellationToken));

        return bundle;
    }

    private async Task<Guid?> FetchChecklistGuidAsync(PunchItemImportMessage message,
        CancellationToken cancellationToken)
        => await importDataFetcher.GetCheckListGuidByCheckListMetaInfo(message, cancellationToken);

    private async Task<IReadOnlyCollection<LibraryItem>> FetchLibraryItemsForPlantAsync(string plant, List<LibraryItemByPlant> keys,
        CancellationToken cancellationToken)
    {
        var libraryItems = await importDataFetcher
            .FetchLibraryItemsForPlantAsync(plant, keys, cancellationToken);
        return libraryItems;
    }

    private async Task<IReadOnlyCollection<Person>> FetchImportUserAsync(CancellationToken cancellationToken)
    {
        var persons = await importDataFetcher.FetchImportUserPersonsAsync(cancellationToken);
        return persons;
    }

    private async Task<Project?> FetchProjectsAsync(ProjectByPlantKey key,
        CancellationToken cancellationToken)
    {
        var project = await importDataFetcher.FetchProjectsAsync(key, cancellationToken);
        return project;
    }

    private async Task<Project?> FetchProjectByGuidAsync(Guid guid, string plant,
        CancellationToken cancellationToken)
    {
        var project = await importDataFetcher.FetchProjectByGuidAsync(guid, plant, cancellationToken);
        return project;
    }
}
