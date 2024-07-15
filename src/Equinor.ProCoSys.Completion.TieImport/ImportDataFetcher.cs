using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.Tags;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.TieImport;

public interface IImportDataFetcher
{
    Task<IReadOnlyCollection<Project>> FetchProjectsAsync(IReadOnlyCollection<ProjectByPlantKey> keys,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Person>> FetchPersonsAsync(IReadOnlyCollection<PersonKey> keys,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<LibraryItem>> FetchLibraryItemsForPlantAsync(IReadOnlyCollection<LibraryItemByPlant> keys,
        CancellationToken cancellationToken);

    IReadOnlyCollection<Task<IReadOnlyCollection<TagCheckList>>> CreateFetchTagsByPlantTasks(
        IReadOnlyCollection<TagNoByProjectNameAndPlantKey> keys,
        CancellationToken cancellationToken);
}

public sealed class ImportDataFetcher(
    CompletionContext completionContext,
    ICheckListCache checkListCache,
    ITagService tagService) : IImportDataFetcher
{
    public async Task<IReadOnlyCollection<Project>> FetchProjectsAsync(IReadOnlyCollection<ProjectByPlantKey> keys,
        CancellationToken cancellationToken)
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
            // ReSharper disable once CoVariantArrayConversion
            .FromSqlRaw(query, parameters.ToArray())
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        return items;
    }

    public async Task<IReadOnlyCollection<Person>> FetchPersonsAsync(IReadOnlyCollection<PersonKey> keys,
        CancellationToken cancellationToken)
    {
        var emailKeys = keys.Select(x => x.Email)
            .Distinct()
            .ToArray();

        var persons = await completionContext.Persons
            .Where(x => emailKeys.Contains(x.Email))
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        return persons;
    }

    public async Task<IReadOnlyCollection<LibraryItem>> FetchLibraryItemsForPlantAsync(
        IReadOnlyCollection<LibraryItemByPlant> keys, CancellationToken cancellationToken)
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
            // ReSharper disable once CoVariantArrayConversion
            .FromSqlRaw(query, parameters.ToArray())
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        return items;
    }

    public IReadOnlyCollection<Task<IReadOnlyCollection<TagCheckList>>> CreateFetchTagsByPlantTasks(
        IReadOnlyCollection<TagNoByProjectNameAndPlantKey> keys, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<IReadOnlyCollection<TagCheckList>>>();
        var byPlant = keys.GroupBy(x => x.Plant);

        foreach (var plantKeys in byPlant)
        {
            var byProject = plantKeys.GroupBy(x => x.ProjectName);
            tasks.AddRange(byProject.Select(projectKeys =>
                CreateFetchTagsByPlantTask(plantKeys.Key, projectKeys.Key, projectKeys.ToArray(), cancellationToken)));
        }

        return tasks;
    }

    private async Task<IReadOnlyCollection<TagCheckList>> CreateFetchTagsByPlantTask(string plant, string projectName,
        TagNoByProjectNameAndPlantKey[] keys, CancellationToken cancellationToken)
    {
        var tagNos = keys.Select(x => x.TagNo).ToArray();
        var tags = await tagService.GetTagsByTagNosAsync(plant, projectName, tagNos, cancellationToken);

        var tasks = tags.Select(tag =>
            checkListCache.GetCheckListsByTagIdAsync(tag.Id, plant, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(x => x)
            .Where(x => keys.Any(y => y.FormType == x.FormularType))
            .ToArray();
    }
}
