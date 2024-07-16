using System.Diagnostics;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class PlantScopedImportDataContextBuilder(IImportDataFetcher importDataFetcher)
    : IScopedContextLibraryTypeBuilder
{
    private Dictionary<string, PlantScopedImportDataContext> _plantScopedImportDataContexts = new();

    public async Task<Dictionary<string, PlantScopedImportDataContext>> BuildAsync(
        IReadOnlyCollection<PunchItemImportMessage> importMessages, CancellationToken cancellationToken)
    {
        await BuildLibraryItemsAsync(importMessages, cancellationToken);

        return _plantScopedImportDataContexts;
    }

    private async Task BuildLibraryItemsAsync(IReadOnlyCollection<PunchItemImportMessage> importMessages,
        CancellationToken cancellationToken)
    {
        var tagNoByPlantKeys = FetchKeysCreator.CreateTagKeys(importMessages);
        var libraryItemsByPlant = FetchKeysCreator.CreateLibraryItemKeys(importMessages);
        var personByEmailKeys = FetchKeysCreator.CreatePersonKeys(importMessages);
        var projectByPlantKeys = FetchKeysCreator
            .CreateProjectKeys(importMessages)
            .ToList();

        _plantScopedImportDataContexts = importMessages
                .GroupBy(x => x.Plant)
                .Select(x => new { x.Key, Value = new PlantScopedImportDataContext(x.Key) })
                .ToDictionary(k => k.Key, v => v.Value)
            ;

        var tagTasks = CreateFetchTagsByPlantTasks(tagNoByPlantKeys.Distinct().ToArray(), cancellationToken);
        await FetchLibraryItemsForPlantAsync(
            libraryItemsByPlant.SelectMany(x => x).ToList(),
            cancellationToken);
        await FetchPersonsAsync(personByEmailKeys.SelectMany(x => x).ToArray(), cancellationToken);
        await FetchProjectsAsync(projectByPlantKeys, cancellationToken);
        await WhenAllFetchTagsByPlantTasksAsync(tagTasks);
    }

    private async Task WhenAllFetchTagsByPlantTasksAsync(
        IReadOnlyCollection<Task<IReadOnlyCollection<TagCheckList>>> tagTasks)
    {
        var results = await Task.WhenAll(tagTasks);
        var checkListsByPlant = results
            .SelectMany(x => x)
            .GroupBy(x => x.Plant);

        foreach (var checkLists in checkListsByPlant)
        {
            if (!_plantScopedImportDataContexts.ContainsKey(checkLists.Key))
            {
                Debugger.Break();
            }
            _plantScopedImportDataContexts[checkLists.Key].AddCheckList(checkLists.ToArray());
        }
    }

    private IReadOnlyCollection<Task<IReadOnlyCollection<TagCheckList>>> CreateFetchTagsByPlantTasks(
        TagNoByProjectNameAndPlantKey[] keys,
        CancellationToken cancellationToken) =>
        importDataFetcher.CreateFetchTagsByPlantTasks(keys, cancellationToken);

    private async Task FetchLibraryItemsForPlantAsync(List<LibraryItemByPlant> keys,
        CancellationToken cancellationToken)
    {
        var libraryItems = await importDataFetcher
            .FetchLibraryItemsForPlantAsync(keys, cancellationToken);

        var byPlant = libraryItems.GroupBy(x => x.Plant);
        foreach (var libraryItem in byPlant)
        {
            _plantScopedImportDataContexts[libraryItem.Key].AddLibraryItems(libraryItem.ToArray());
        }
    }

    private async Task FetchPersonsAsync(IReadOnlyCollection<PersonKey> keys, CancellationToken cancellationToken)
    {
        var persons = await importDataFetcher.FetchPersonsAsync(keys, cancellationToken);

        foreach (var scopedContext in _plantScopedImportDataContexts)
        {
            scopedContext.Value.AddPersons(persons);
        }
    }

    private async Task FetchProjectsAsync(IReadOnlyCollection<ProjectByPlantKey> keys,
        CancellationToken cancellationToken)
    {
        var projects = await importDataFetcher.FetchProjectsAsync(keys, cancellationToken);

        var byPlant = projects.GroupBy(x => x.Plant);
        foreach (var project in byPlant)
        {
            _plantScopedImportDataContexts[project.Key].AddProjects(project.ToArray());
        }
    }
}

public interface IScopedContextLibraryTypeBuilder
{
}
