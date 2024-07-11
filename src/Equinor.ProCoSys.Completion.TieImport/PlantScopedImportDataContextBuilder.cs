using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.Infrastructure;
using MassTransit.Util;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Graph.Models;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class PlantScopedImportDataContextBuilder(CompletionContext completionContext)
    : IScopedContextLibraryTypeBuilder
{
    private readonly Dictionary<string, List<LibraryItemByPlant>> _libraryItemsByPlant = new();
    private readonly Dictionary<string, PersonKey> _personByEmail = new ();
    private Dictionary<string, PlantScopedImportDataContext> _plantScopedImportDataContexts = new();
    private List<ProjectByPlantKey> _projectsByPlant = new();

    public async Task<Dictionary<string, PlantScopedImportDataContext>> BuildAsync(
        IReadOnlyCollection<PunchItemImportMessage> importMessages, CancellationToken cancellationToken)
    {
        await BuildLibraryItemsAsync(importMessages, cancellationToken);

        return _plantScopedImportDataContexts;
    }

    private async Task BuildLibraryItemsAsync(IReadOnlyCollection<PunchItemImportMessage> importMessages,
        CancellationToken cancellationToken)
    {
        foreach (var message in importMessages)
        {
            CreateLibraryItemKeys(message.PunchListType, message.Plant);
            CreateLibraryItemKeys(message.ClearedByOrganization, message.Plant);
            CreateLibraryItemKeys(message.RaisedByOrganization, message.Plant);
            CreatePersonKeys(message.ClearedBy);
            CreatePersonKeys(message.VerifiedBy);
            CreatePersonKeys(message.RejectedBy);
            CreateProjectKeys(message, message.Plant);
        }

        _plantScopedImportDataContexts = importMessages
                .GroupBy(x => x.Plant)
                .Select(x => new { x.Key, Value = new PlantScopedImportDataContext() })
                .ToDictionary(k => k.Key, v => v.Value)
            ;

        await FetchLibraryItemsForPlantAsync(_libraryItemsByPlant.Values.SelectMany(x => x).ToList(), cancellationToken);
        await FetchPersons(_personByEmail.Values.ToArray(), cancellationToken);
        await FetchProjects(_projectsByPlant, cancellationToken);
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
    
    private async Task FetchPersons(PersonKey[] keys, CancellationToken cancellationToken)
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
    
    private async Task FetchProjects(List<ProjectByPlantKey> keys, CancellationToken cancellationToken)
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
    
    private void CreateLibraryItemKeys(Optional<string?> libraryCode, string plant)
    {
        if (libraryCode is not { HasValue: true, Value: not null })
        {
            return;
        }

        var key = new LibraryItemByPlant(libraryCode.Value, LibraryType.COMPLETION_ORGANIZATION,
            plant);

        if (_libraryItemsByPlant.TryGetValue(key.Plant, out var value))
        {
            value.Add(key);
        }
        else
        {
            _libraryItemsByPlant[key.Plant] = [key];
        }
    }
    
    private void CreatePersonKeys(Optional<string?> personEmail)
    {
        if (personEmail is not { HasValue: true, Value: not null })
        {
            return;
        }

        var key = new PersonKey(personEmail.Value);
        _personByEmail[key.Email] = key;
    }
    
    private void CreateProjectKeys(PunchItemImportMessage message, string plant)
    {
        var key = new ProjectByPlantKey(message.Project, plant);
        _projectsByPlant.Add(key);
    }
}

public interface IScopedContextLibraryTypeBuilder
{
}
