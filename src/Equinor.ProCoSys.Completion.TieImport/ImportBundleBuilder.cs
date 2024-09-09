using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class ImportBundleBuilder(IImportDataFetcher importDataFetcher)
{
    public async Task<ImportDataBundle> BuildAsync(
        PunchItemImportMessage importMessage, CancellationToken cancellationToken)
    {
        var libraryItemsByPlant = FetchKeysCreator.CreateLibraryItemKeys(importMessage);
        var projectByPlantKey = FetchKeysCreator
            .CreateProjectKeys(importMessage);
        var context = new ImportDataBundle(importMessage.Plant);
        context.AddLibraryItems(await FetchLibraryItemsForPlantAsync(
            libraryItemsByPlant.SelectMany(x => x).ToList(),
            cancellationToken));
        context.AddProjects(await FetchProjectsAsync(projectByPlantKey, cancellationToken));
        context.CheckListGuid = await FetchChecklistGuidsAsync(importMessage);
        context.AddPersons(await FetchImportUserAsync( cancellationToken));

        return context;
    }

    private async Task<Guid?> FetchChecklistGuidsAsync(PunchItemImportMessage message)
        => await importDataFetcher.GetCheckListGuidByCheckListMetaInfo(message, CancellationToken.None);

    private async Task<IReadOnlyCollection<LibraryItem>> FetchLibraryItemsForPlantAsync(List<LibraryItemByPlant> keys,
        CancellationToken cancellationToken)
    {
        var libraryItems = await importDataFetcher
            .FetchLibraryItemsForPlantAsync(keys, cancellationToken);

        return libraryItems;
    }

    private async Task<IReadOnlyCollection<Person>> FetchImportUserAsync(CancellationToken cancellationToken)
    {
        var persons = await importDataFetcher.FetchImportUserPersonsAsync(cancellationToken);
        return persons;
    }

    private async Task<IReadOnlyCollection<Project>> FetchProjectsAsync(ProjectByPlantKey key,
        CancellationToken cancellationToken)
    {
        var projects = await importDataFetcher.FetchProjectsAsync(key, cancellationToken);
        return projects;
    }
}
