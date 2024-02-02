using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.LinkEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.Links;

public class LinkService : ILinkService
{
    private readonly ILinkRepository _linkRepository;
    private readonly IPlantProvider _plantProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<LinkService> _logger;

    public LinkService(
        ILinkRepository linkRepository,
        IPlantProvider plantProvider,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<LinkService> logger)
    {
        _linkRepository = linkRepository;
        _plantProvider = plantProvider;
        _unitOfWork = unitOfWork;
        _integrationEventPublisher = integrationEventPublisher;
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

        // ReSharper disable once UnusedVariable
        var integrationEvent = await PublishCreatedEventsAsync(link, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Todo 109496 sync with pcs4 goes here
        // await _syncToPCS4Service.SyncNewObjectAsync(SyncToPCS4Service.Link, integrationEvent, link.Plant, cancellationToken);

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
        // ReSharper disable once NotAccessedVariable
        LinkUpdatedIntegrationEvent integrationEvent;
        if (changes.Any())
        {
            // ReSharper disable once RedundantAssignment
            integrationEvent = await PublishUpdatedEventsAsync(link, changes, cancellationToken);
        }

        link.SetRowVersion(rowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Todo 109496 sync with pcs4 goes here
        // await _syncToPCS4Service.SyncObjectUpdateAsync(SyncToPCS4Service.Link, integrationEvent, link.Plant, cancellationToken);

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

        _linkRepository.Remove(link);

        // ReSharper disable once UnusedVariable
        var integrationEvent = await PublishDeletedEventsAsync(link, cancellationToken);

        // Setting RowVersion before delete has 2 missions:
        // 1) Set correct Concurrency
        // 2) Ensure that _unitOfWork.SetAuditDataAsync can set ModifiedBy / ModifiedAt needed in published events
        link.SetRowVersion(rowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Todo 109496 sync with pcs4 goes here
        // await _syncToPCS4Service.SyncObjectDeletionAsync(SyncToPCS4Service.Link, integrationEvent, link.Plant, cancellationToken);

        _logger.LogInformation("Link '{LinkTitle}' with guid: {LinkGuid} deleted for {ParentType} : {LinkParentGuid}", 
            link.Title, 
            link.Guid,
            link.ParentType, 
            link.ParentGuid);
    }

    private async Task<LinkCreatedIntegrationEvent> PublishCreatedEventsAsync(Link link, CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = new LinkCreatedIntegrationEvent(link, _plantProvider.Plant);
        await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var properties = new List<IProperty>
        {
            new Property(nameof(Link.Title), link.Title),
            new Property(nameof(Link.Url), link.Url)
        };
        var historyEvent = new HistoryCreatedIntegrationEvent(
            $"Link {link.Title} created",
            link.Guid,
            link.ParentGuid,
            new User(link.CreatedBy.Guid, link.CreatedBy.GetFullName()),
            link.CreatedAtUtc,
            properties);
        await _integrationEventPublisher.PublishAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private async Task<LinkUpdatedIntegrationEvent> PublishUpdatedEventsAsync(
        Link link,
        List<IChangedProperty> changes,
        CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = new LinkUpdatedIntegrationEvent(link, _plantProvider.Plant);
        await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryUpdatedIntegrationEvent(
            $"Link {link.Title} updated",
            link.Guid,
            link.ParentGuid,
            new User(link.ModifiedBy!.Guid, link.ModifiedBy!.GetFullName()),
            link.ModifiedAtUtc!.Value,
            changes);
        await _integrationEventPublisher.PublishAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private async Task<LinkDeletedIntegrationEvent> PublishDeletedEventsAsync(Link link, CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = new LinkDeletedIntegrationEvent(link, _plantProvider.Plant);
        await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryDeletedIntegrationEvent(
            $"Link {link.Title} deleted",
            link.Guid,
            link.ParentGuid,
            // Our entities don't have DeletedByOid / DeletedAtUtc ...
            // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
            new User(link.ModifiedBy!.Guid, link.ModifiedBy!.GetFullName()),
            link.ModifiedAtUtc!.Value);
        await _integrationEventPublisher.PublishAsync(historyEvent, cancellationToken);
        return integrationEvent;
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
