using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.TieImport;

public interface IImportDataFetcher
{
    Task<Project> FetchProjectsAsync(ProjectByPlantKey key,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Person>> FetchImportUserPersonsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<LibraryItem>> FetchLibraryItemsForPlantAsync(IReadOnlyCollection<LibraryItemByPlant> keys,
        CancellationToken cancellationToken);
    
    Task<Guid?> GetCheckListGuidByCheckListMetaInfo(
        PunchItemImportMessage message, CancellationToken cancellationToken);
}

public sealed class ImportDataFetcher(
    CompletionContext completionContext,
    ICheckListCache checkListCache,
    IOptionsMonitor<ImportUserOptions> importOptions
    ) : IImportDataFetcher
{
    public async Task<Project> FetchProjectsAsync(ProjectByPlantKey key,
        CancellationToken cancellationToken)
    {
        var items = await completionContext.Projects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(p => p.Name == key.Project && p.Plant == key.Plant,cancellationToken);

        return items;
    }

    public async Task<IReadOnlyCollection<Person>> FetchImportUserPersonsAsync(
        CancellationToken cancellationToken)
    {
        var importUserName = importOptions.CurrentValue.ImportUserName;
        var persons = await completionContext.Persons
            .Where(x =>  importUserName == x.UserName)
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

    public async Task<Guid?> GetCheckListGuidByCheckListMetaInfo(
        PunchItemImportMessage message, CancellationToken cancellationToken) =>
        await checkListCache.GetCheckListGuidByMetaInfoAsync(message.Plant, message.TagNo, message.Responsible, message.FormType, cancellationToken);
}
