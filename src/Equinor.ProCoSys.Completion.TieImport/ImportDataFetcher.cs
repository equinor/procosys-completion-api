using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
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

    Task<PunchItem[]> FetchExternalPunchItemNosAsync(IReadOnlyCollection<ExternalPunchItemKey> keys,
        IReadOnlyCollection<TagCheckList> checkLists,
        CancellationToken cancellationToken);
}

public sealed class ImportDataFetcher(
    CompletionContext completionContext,
    //ICheckListCache checkListCache,
    ICheckListApiService checkListService) : IImportDataFetcher
{
    public async Task<IReadOnlyCollection<Project>> FetchProjectsAsync(IReadOnlyCollection<ProjectByPlantKey> keys,
        CancellationToken cancellationToken)
    {
        keys = keys.Distinct().ToList();
        if (keys.Count == 0) return [];

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
            .Where(x => x is not null)
            .Distinct()
            .ToArray();

        var usernameKeys = keys.Select(x => x.Username)
            .Where(x => x is not null)
            .Distinct()
            .ToArray();

        var persons = await completionContext.Persons
            .Where(x => emailKeys.Contains(x.Email) || usernameKeys.Contains(x.UserName))
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
        if (keys.Count == 0)
        {
            return [];
        }

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

    public async Task<PunchItem[]> FetchExternalPunchItemNosAsync(IReadOnlyCollection<ExternalPunchItemKey> keys,
        IReadOnlyCollection<TagCheckList> checkLists,
        CancellationToken cancellationToken)
    {
        var externalItemNos = keys.Select(y => y.ExternalPunchItemNo).ToList();
        var plants = keys.Select(y => y.Plant).ToList();
        var projectNames = keys.Select(y => y.ProjectName).ToList();

        
        //TODO expand to handle PunchItemNo messages also if we do Update
        var punchItems = await completionContext.PunchItems
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.RaisedByOrg)
            .Include(x => x.ClearingByOrg)
            .Include(x => x.Type)
            .Include(x => x.ClearedBy)
            .Include(x => x.VerifiedBy)
            .Include(x => x.RejectedBy)
            .Include(x => x.Project)
            .Where(x => !string.IsNullOrEmpty(x.ExternalItemNo)
                        && externalItemNos.Contains(x.ExternalItemNo)
                        && plants.Contains(x.Plant)
                        && projectNames.Contains(x.Project.Name))
            .ToArrayAsync(cancellationToken);

        var byPlant = punchItems.GroupBy(x => x.Plant);

        var punchItemsByExternalPunchItemKey = new List<PunchItem>();
        foreach (var plantPunches in byPlant)
        {
            var filteredPunches = plantPunches
                .Select(p => new
                {
                    Punch = p,
                    Key = keys.First(k =>
                        k.ExternalPunchItemNo == p.ExternalItemNo && k.Plant == p.Plant &&
                        k.ProjectName == p.Project.Name)
                })
                .Where(pk => checkLists.Any(c =>
                    c.Plant == pk.Key.Plant && c.ProCoSysGuid == pk.Punch.CheckListGuid && c.ResponsibleCode ==
                    pk.Key.Responsible))
                .Select(x => x.Punch);
            punchItemsByExternalPunchItemKey.AddRange(filteredPunches);
        }

        return punchItemsByExternalPunchItemKey.ToArray();
    }

    private async Task<IReadOnlyCollection<TagCheckList>> CreateFetchTagsByPlantTask(string plant, string projectName,
        TagNoByProjectNameAndPlantKey[] keys, CancellationToken cancellationToken)
    {

        throw new NotImplementedException();
        //var tagNos = keys.Select(x => x.TagNo).ToArray();
        //var tags = await tagService.GetTagsByTagNosAsync(plant, projectName, tagNos, cancellationToken);

        //var tasks = tags.Select(tag =>
        //    checkListCache.GetCheckListsByTagIdAsync(tag.Id, plant, cancellationToken));

        //var results = await Task.WhenAll(tasks);

        //return results
        //    .SelectMany(x => x) // Flatten
        //    .Where(x => keys.Any(y => y.FormType == x.FormularType)) //if we had more specific webApi endpoint, this is not needed
        //    .ToArray();
    }
}
