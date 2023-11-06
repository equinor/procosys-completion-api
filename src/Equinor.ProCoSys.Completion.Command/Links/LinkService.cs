using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.Links;

public class LinkService : ILinkService
{
    private readonly ILinkRepository _linkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LinkService> _logger;

    public LinkService(
        ILinkRepository linkRepository,
        IUnitOfWork unitOfWork,
        ILogger<LinkService> logger)
    {
        _linkRepository = linkRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LinkDto> AddAsync(
        string sourceType,
        Guid sourceGuid,
        string title,
        string url,
        CancellationToken cancellationToken)
    {
        var link = new Link(sourceType, sourceGuid, title, url);
        _linkRepository.Add(link);
        link.AddDomainEvent(new LinkCreatedDomainEvent(link));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Link '{LinkTitle}' with guid: {LinkGuid} created for {SourceType} : {LinkSourceGuid}", 
            link.Title, 
            link.Guid,
            link.SourceType, 
            link.SourceGuid);

        return new LinkDto(link.Guid, link.RowVersion.ConvertToString());
    }

    public async Task<bool> ExistsAsync(Guid guid, CancellationToken cancellationToken)
        => await _linkRepository.ExistsAsync(guid, cancellationToken);

    public async Task<string> UpdateAsync(
        Guid guid,
        string title,
        string url,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        var link = await _linkRepository.GetAsync(guid, cancellationToken);

        var changes = UpdateLink(link, title, url);
        if (changes.Any())
        {
            link.AddDomainEvent(new LinkUpdatedDomainEvent(link, changes));
        }

        link.SetRowVersion(rowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Link '{LinkTitle}' with guid: {LinkGuid} updated for {SourceType} : {LinkSourceGuid}", 
            link.Title, 
            link.Guid,
            link.SourceType, 
            link.SourceGuid);

        return link.RowVersion.ConvertToString();
    }

    public async Task DeleteAsync(
        Guid guid,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        var link = await _linkRepository.GetAsync(guid, cancellationToken);

        // Setting RowVersion before delete has 2 missions:
        // 1) Set correct Concurrency
        // 2) Trigger the update of modifiedBy / modifiedAt to be able to log who performed the deletion
        link.SetRowVersion(rowVersion);
        _linkRepository.Remove(link);
        link.AddDomainEvent(new LinkDeletedDomainEvent(link));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Link '{LinkTitle}' with guid: {LinkGuid} deleted for {SourceType} : {LinkSourceGuid}", 
            link.Title, 
            link.Guid,
            link.SourceType, 
            link.SourceGuid);
    }

    private List<IProperty> UpdateLink(Link link, string title, string url)
    {
        var changes = new List<IProperty>();

        if (link.Url != url)
        {
            changes.Add(new Property<string>(
                nameof(Link.Url), 
                link.Url,
                url));
            link.Url = url;
        }
        if (link.Title != title)
        {
            changes.Add(new Property<string>(
                nameof(Link.Title),
                link.Title,
                title));
            link.Title = title;
        }

        return changes;
    }
}
