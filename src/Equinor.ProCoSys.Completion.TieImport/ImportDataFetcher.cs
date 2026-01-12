using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.TieImport;

public interface IImportDataFetcher
{
    Task<Project?> FetchProjectsAsync(ProjectByPlantKey key,
        CancellationToken cancellationToken);

    Task<Project?> FetchProjectByGuidAsync(Guid guid, string plant,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Person>> FetchImportUserPersonsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<LibraryItem>> FetchLibraryItemsForPlantAsync(string plant,
        IReadOnlyCollection<LibraryItemByPlant> keys,
        CancellationToken cancellationToken);
    
    Task<Guid?> GetCheckListGuidByCheckListMetaInfo(
        PunchItemImportMessage message, CancellationToken cancellationToken);

    Task<ProCoSys4CheckList?> GetCheckListByGuidAsync(
        Guid checkListGuid, CancellationToken cancellationToken);
}

public sealed class ImportDataFetcher(
    CompletionContext completionContext,
    ICheckListApiService checkListApiService
    ) : IImportDataFetcher
{
    public async Task<Project?> FetchProjectsAsync(ProjectByPlantKey key,
        CancellationToken cancellationToken)
    {
        var items = await completionContext.Projects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Name == key.Project && p.Plant == key.Plant,cancellationToken);

        return items;
    }

    public async Task<Project?> FetchProjectByGuidAsync(Guid guid, string plant,
        CancellationToken cancellationToken)
    {
        var project = await completionContext.Projects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Guid == guid && p.Plant == plant, cancellationToken);
        return project;
    }

    public async Task<IReadOnlyCollection<Person>> FetchImportUserPersonsAsync(
        CancellationToken cancellationToken)
    {
        const string ImportUserName = ImportUserOptions.UserName;
        var persons = await completionContext.Persons
            .Where(x =>  ImportUserName == x.UserName)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        return persons;
    }

    public async Task<IReadOnlyCollection<LibraryItem>> FetchLibraryItemsForPlantAsync(
        string plant,
        IReadOnlyCollection<LibraryItemByPlant> keys, CancellationToken cancellationToken)
    {
        if (keys.Count == 0)
        {
            return [];
        } 
        
        //I don't particularly like this, but I'm unable to find a better alternative.
        //Alternative approach would be to filter in memory, but I'm uncertain how well that would scale.
        var keyStrings = keys.Select(key => string.Join(",",key.Code , key.Type)).ToList(); 
        return await completionContext.Library
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.Plant == plant && keyStrings.Any(k => k == l.Code+","+ l.Type))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> GetCheckListGuidByCheckListMetaInfo(
        PunchItemImportMessage message, CancellationToken cancellationToken) =>
        await checkListApiService.GetCheckListGuidByMetaInfoAsync(message.Plant, message.TagNo, message.Responsible, message.FormType, message.ProjectName, cancellationToken);

    public async Task<ProCoSys4CheckList?> GetCheckListByGuidAsync(
        Guid checkListGuid, CancellationToken cancellationToken) =>
        await checkListApiService.GetCheckListAsync(checkListGuid, cancellationToken);
}
