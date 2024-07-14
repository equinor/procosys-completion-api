using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.Extensions;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.Tags;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class PlantScopedImportDataContextBuilder(CompletionContext completionContext, ICheckListCache checkListCache, ITagService tagService)
    : IScopedContextLibraryTypeBuilder
{
    private ILookup<string, LibraryItemByPlant> _libraryItemsByPlant = EmptyLookup<string, LibraryItemByPlant>.Empty();
    private ILookup<string, PersonKey> _personByEmail = EmptyLookup<string, PersonKey>.Empty();
    private Dictionary<string, PlantScopedImportDataContext> _plantScopedImportDataContexts = new();
    private readonly List<ProjectByPlantKey> _projectsByPlant = [];
    private readonly List<TagNoByProjectNameAndPlantKey> _tagNoByPlantKeys = [];

    public async Task<Dictionary<string, PlantScopedImportDataContext>> BuildAsync(
        IReadOnlyCollection<PunchItemImportMessage> importMessages, CancellationToken cancellationToken)
    {
        await BuildLibraryItemsAsync(importMessages, cancellationToken);

        return _plantScopedImportDataContexts;
    }

    private async Task BuildLibraryItemsAsync(IReadOnlyCollection<PunchItemImportMessage> importMessages,
        CancellationToken cancellationToken)
    {
        _tagNoByPlantKeys.AddRange(FetchKeysCreator.CreateTagKeys(importMessages));
        _libraryItemsByPlant = FetchKeysCreator.CreateLibraryItemKeys(importMessages);
        _personByEmail = FetchKeysCreator.CreatePersonKeys(importMessages);
        _projectsByPlant .AddRange(FetchKeysCreator.CreateProjectKeys(importMessages));

        _plantScopedImportDataContexts = importMessages
                .GroupBy(x => x.Plant)
                .Select(x => new { x.Key, Value = new PlantScopedImportDataContext() })
                .ToDictionary(k => k.Key, v => v.Value)
            ;

        var tagTasks = CreateFetchTagsByPlantTasks(_tagNoByPlantKeys.Distinct().ToArray(), cancellationToken);
        await FetchLibraryItemsForPlantAsync(
            _libraryItemsByPlant.SelectMany(x => x).ToList(),
            cancellationToken);
        await FetchPersonsAsync(_personByEmail.SelectMany(x => x).ToArray(), cancellationToken);
        await FetchProjectsAsync(_projectsByPlant, cancellationToken);
        await WhenAllFetchTagsByPlantTasksAsync(tagTasks);
    }

    private async Task WhenAllFetchTagsByPlantTasksAsync(Task<TagCheckList[]>[] tagTasks)
    {
        var results = await Task.WhenAll(tagTasks);
        var checkListsByPlant = results
            .SelectMany(x => x)
            .GroupBy(x => x.Plant);
        
        foreach (var checkLists in checkListsByPlant)
        {
            _plantScopedImportDataContexts[checkLists.Key].AddCheckList(checkLists.ToArray());
        }
    }

    private Task<TagCheckList[]>[] CreateFetchTagsByPlantTasks(TagNoByProjectNameAndPlantKey[] keys, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<TagCheckList[]>>();
        var byPlant = keys.GroupBy(x => x.Plant);

        foreach (var plantKeys in byPlant)
        {
            var byProject = plantKeys.GroupBy(x => x.ProjectName);
            foreach (var projectKeys in byProject)
            {
                var foo = CreateFetchTagsByPlantTask(plantKeys.Key, projectKeys.Key, projectKeys.ToArray(),
                    cancellationToken);
                tasks.Add(foo);
            }
        }

        var fetchTagsByPlantTasks = tasks.ToArray();
        return fetchTagsByPlantTasks;
    }

    private async Task<TagCheckList[]> CreateFetchTagsByPlantTask(string plant, string projectName, TagNoByProjectNameAndPlantKey[] keys, CancellationToken cancellationToken)
    {
        var tagNos = keys.Select(x => x.TagNo).ToArray();
        var tags = await tagService.GetTagsByTagNosAsync(plant, projectName, tagNos, cancellationToken);
        
        var tasks = tags.Select(tag =>
            checkListCache.GetCheckListsByTagIdAsync(tag.Id, plant, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return results.SelectMany(x => x).ToArray();
    }

    private async Task FetchLibraryItemsForPlantAsync(List<LibraryItemByPlant> keys,
        CancellationToken cancellationToken)
    {
        keys = keys
            .Distinct()
            .ToList();
        var query = $"""
                     SELECT Guid, Id, Code, Description, Type, Plant, IsVoided, PeriodEnd, PeriodStart, ProCoSys4LastUpdated, RowVersion, SyncTimestamp
                     FROM Library l
                     WHERE EXISTS (
                           SELECT 1
                           FROM (VALUES {string.Join(",", keys.Select((_, i) => $"(@Code{i}, @Type{i}, @Plant{i})"))}
                                 ) AS t(Code, Type, Plant)
                           WHERE l.Code = t.Code AND l.Type = t.Type AND l.Plant = t.Plant
                       )
                     """;

        var parameters = new List<SqlParameter>();

        parameters.AddRange(keys.Select((k, i) => new SqlParameter($"@Code{i}", k.Code)));
        parameters.AddRange(keys.Select((k, i) => new SqlParameter($"@Type{i}", k.Type.ToString())));
        parameters.AddRange(keys.Select((k, i) => new SqlParameter($"@Plant{i}", k.Plant)));

        var items = await completionContext.Library
            .FromSqlRaw(query, parameters.ToArray())
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        var byPlant = items.GroupBy(x => x.Plant);
        foreach (var libraryItems in byPlant)
        {
            _plantScopedImportDataContexts[libraryItems.Key].AddLibraryItems(libraryItems.ToArray());
        }
    }

    private async Task FetchPersonsAsync(PersonKey[] keys, CancellationToken cancellationToken)
    {
        var emailKeys = keys.Select(x => x.Email)
            .Distinct()
            .ToArray();

        var persons = await completionContext.Persons
            .Where(x => emailKeys.Contains(x.Email))
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        foreach (var scopedContext in _plantScopedImportDataContexts)
        {
            scopedContext.Value.AddPersons(persons);
        }
    }

    private async Task FetchProjectsAsync(List<ProjectByPlantKey> keys, CancellationToken cancellationToken)
    {
        keys = keys.Distinct().ToList();
        var query = $"""
                     SELECT Guid, Id, Name, Description, Plant, IsVoided, IsClosed, IsDeletedInSource, PeriodEnd, PeriodStart, ProCoSys4LastUpdated, RowVersion, SyncTimestamp
                     FROM Projects l
                     WHERE EXISTS (
                           SELECT 1
                           FROM (VALUES {string.Join(",", keys.Select((_, i) => $"(@Name{i}, @Plant{i})"))}
                                 ) AS t(Name, Plant)
                           WHERE l.Name = t.Name AND l.Plant = t.Plant
                       )
                     """;

        var parameters = new List<SqlParameter>();

        parameters.AddRange(keys.Select((k, i) => new SqlParameter($"@Name{i}", k.Project)));
        parameters.AddRange(keys.Select((k, i) => new SqlParameter($"@Plant{i}", k.Plant)));

        var items = await completionContext.Projects
            .FromSqlRaw(query, parameters.ToArray())
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        var byPlant = items.GroupBy(x => x.Plant);
        foreach (var projects in byPlant)
        {
            _plantScopedImportDataContexts[projects.Key].AddProjects(projects.ToArray());
        }
    }

    
}

public interface IScopedContextLibraryTypeBuilder
{
}
