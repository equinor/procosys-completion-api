using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class PlantScopedImportDataContextBuilder(IImportDataFetcher importDataFetcher, IOptionsMonitor<TieImportOptions> tieOptions)
{
    //private Dictionary<string, PlantScopedImportDataContext> _plantScopedImportDataContexts = new();

    public async Task<ImportDataBundle> BuildAsync(
        IReadOnlyCollection<PunchItemImportMessage> importMessages, CancellationToken cancellationToken)
    {
        var tagNoByPlantKeys = FetchKeysCreator.CreateTagKeys(importMessages);
        var libraryItemsByPlant = FetchKeysCreator.CreateLibraryItemKeys(importMessages);
        var personByEmailKeys = FetchKeysCreator.CreatePersonKeys(importMessages, tieOptions.CurrentValue.ImportUserName);
        var projectByPlantKeys = FetchKeysCreator
            .CreateProjectKeys(importMessages)
            .ToList();
        var externalPunchItemNoKeys = FetchKeysCreator.CreateExternalPunchItemNoKeys(importMessages);

        var context = new ImportDataBundle(importMessages.First().TiObject.Site);
        // _plantScopedImportDataContexts = importMessages
        //         .GroupBy(x => x.TiObject.Site)
        //         .Select(x => new { x.Key, Value = new PlantScopedImportDataContext(x.Key) })
        //         .ToDictionary(k => k.Key, v => v.Value);

        var tagTasks = CreateFetchTagsByPlantTasks(tagNoByPlantKeys.Distinct().ToArray(), cancellationToken);
        context.AddLibraryItems(await FetchLibraryItemsForPlantAsync(
            libraryItemsByPlant.SelectMany(x => x).ToList(),
            cancellationToken));
        context.AddPersons(await FetchPersonsAsync(personByEmailKeys, cancellationToken));
        context.AddProjects(await FetchProjectsAsync(projectByPlantKeys, cancellationToken));
        context.AddCheckLists(await WhenAllFetchTagsByPlantTasksAsync(tagTasks));
        context.AddPunchItems(await FetchExternalPunchItemNosAsync(context.CheckLists,externalPunchItemNoKeys, cancellationToken));

        return context;
       // return _plantScopedImportDataContexts;
    }

    private async Task<PunchItem[]> FetchExternalPunchItemNosAsync(List<TagCheckList> checkLists,
        IReadOnlyCollection<ExternalPunchItemKey> externalPunchItemNoKeys,
        CancellationToken cancellationToken)
    {
        var punchItems = await importDataFetcher.FetchExternalPunchItemNosAsync(
            externalPunchItemNoKeys,
            checkLists,
            cancellationToken);
        return punchItems;
    }

    private async Task<IReadOnlyCollection<TagCheckList>> WhenAllFetchTagsByPlantTasksAsync(
        IReadOnlyCollection<Task<IReadOnlyCollection<TagCheckList>>> tagTasks)
    {
        var results = await Task.WhenAll(tagTasks);

        return results.SelectMany(x => x).ToArray();
    }

    private IReadOnlyCollection<Task<IReadOnlyCollection<TagCheckList>>> CreateFetchTagsByPlantTasks(
        TagNoByProjectNameAndPlantKey[] keys,
        CancellationToken cancellationToken) =>
        importDataFetcher.CreateFetchTagsByPlantTasks(keys, cancellationToken);

    private async Task<IReadOnlyCollection<LibraryItem>> FetchLibraryItemsForPlantAsync(List<LibraryItemByPlant> keys,
        CancellationToken cancellationToken)
    {
        var libraryItems = await importDataFetcher
            .FetchLibraryItemsForPlantAsync(keys, cancellationToken);

        return libraryItems;
    }

    private async Task<IReadOnlyCollection<Person>> FetchPersonsAsync(IReadOnlyCollection<PersonKey> keys, CancellationToken cancellationToken)
    {
        var persons = await importDataFetcher.FetchPersonsAsync(keys, cancellationToken);
        return persons;
    }

    private async Task<IReadOnlyCollection<Project>> FetchProjectsAsync(IReadOnlyCollection<ProjectByPlantKey> keys,
        CancellationToken cancellationToken)
    {
        var projects = await importDataFetcher.FetchProjectsAsync(keys, cancellationToken);
        return projects;
    }
}
