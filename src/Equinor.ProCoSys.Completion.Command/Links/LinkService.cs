using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.LinkDomainEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts.History;
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
        string parentType,
        Guid parentGuid,
        string title,
        string url,
        CancellationToken cancellationToken)
    {
        var link = new Link(parentType, parentGuid, title, url);
        _linkRepository.Add(link);
        link.AddDomainEvent(new LinkCreatedDomainEvent(link));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Link '{LinkTitle}' with guid: {LinkGuid} created for {ParentType} : {LinkParentGuid}", 
            link.Title, 
            link.Guid,
            link.ParentType, 
            link.ParentGuid);

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

        _logger.LogInformation("Link '{LinkTitle}' with guid: {LinkGuid} updated for {ParentType} : {LinkParentGuid}", 
            link.Title, 
            link.Guid,
            link.ParentType, 
            link.ParentGuid);

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
        // 2) Ensure that _unitOfWork.SetAuditDataAsync can set ModifiedBy / ModifiedAt needed in published events
        link.SetRowVersion(rowVersion);
        _linkRepository.Remove(link);
        link.AddDomainEvent(new LinkDeletedDomainEvent(link));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Link '{LinkTitle}' with guid: {LinkGuid} deleted for {ParentType} : {LinkParentGuid}", 
            link.Title, 
            link.Guid,
            link.ParentType, 
            link.ParentGuid);
    }

    private List<IChangedProperty> UpdateLink(Link link, string title, string url)
    {
        var changes = new List<IChangedProperty>();

        if (link.Url != url)
        {
            changes.Add(new ChangedProperty<string>(
                nameof(Link.Url), 
                link.Url,
                url));
            link.Url = url;
        }
        if (link.Title != title)
        {
            changes.Add(new ChangedProperty<string>(
                nameof(Link.Title),
                link.Title,
                title));
            link.Title = title;
        }

        return changes;
    }
}
