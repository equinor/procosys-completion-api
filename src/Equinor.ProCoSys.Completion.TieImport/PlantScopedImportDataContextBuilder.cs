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
    public async Task<ImportDataBundle> BuildAsync(
        IReadOnlyCollection<PunchItemImportMessage> importMessages, CancellationToken cancellationToken)
    {
        var libraryItemsByPlant = FetchKeysCreator.CreateLibraryItemKeys(importMessages);
        var projectByPlantKeys = FetchKeysCreator
            .CreateProjectKeys(importMessages)
            .ToList();
        // var personByEmailKeys = FetchKeysCreator.CreatePersonKeys(importMessages, tieOptions.CurrentValue.ImportUserName);
        //var externalPunchItemNoKeys = FetchKeysCreator.CreateExternalPunchItemNoKeys(importMessages);

        var context = new ImportDataBundle(importMessages.First().TiObject.Site);
        context.AddLibraryItems(await FetchLibraryItemsForPlantAsync(
            libraryItemsByPlant.SelectMany(x => x).ToList(),
            cancellationToken));
        context.AddProjects(await FetchProjectsAsync(projectByPlantKeys, cancellationToken));
        context.AddCheckLists(await FetchChecklistGuidsAsync(importMessages));
        //context.AddPersons(await FetchPersonsAsync(personByEmailKeys, cancellationToken));
        //context.AddPunchItems(await GetPunchItemNoByExternalNoAndChecklist(context.CheckListGuids,externalPunchItemNoKeys, cancellationToken));

        return context;
    }
    
    //TODO fix when update
    // private async Task<PunchItem[]> GetPunchItemNoByExternalNoAndChecklist(List<Guid?> checkLists,
    //     IReadOnlyCollection<ExternalPunchItemKey> externalPunchItemNoKeys,
    //     CancellationToken cancellationToken)
    // {
    //     var punchItems = await importDataFetcher.FetchExternalPunchItemNosAsync(
    //         externalPunchItemNoKeys,
    //         checkLists,
    //         cancellationToken);
    //     return punchItems;
    // }

    private async Task<List<Guid?>> FetchChecklistGuidsAsync(IEnumerable<PunchItemImportMessage> messages)
    {
        var results = new List<Guid?>();
        
        foreach (var punchItemImportMessage in messages)
        {
            var checklistGuid = await importDataFetcher.GetCheckListGuidByCheckListMetaInfo(punchItemImportMessage, CancellationToken.None);
            results.Add(checklistGuid);
        }
        return results;
    }

    // private IReadOnlyCollection<Task<IReadOnlyCollection<TagCheckList>>> CreateFetchTagsByPlantTasks(
    //     TagNoByProjectNameAndPlantKey[] keys,
    //     CancellationToken cancellationToken) =>
    //     importDataFetcher.GetCheckListGuidsByCheckListMetaInfo(keys, cancellationToken);

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
