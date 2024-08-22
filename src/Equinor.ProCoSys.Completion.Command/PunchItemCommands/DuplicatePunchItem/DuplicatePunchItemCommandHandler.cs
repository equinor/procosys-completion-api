using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;

public class DuplicatePunchItemCommandHandler : IRequestHandler<DuplicatePunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IMessageProducer _messageProducer;
    private readonly ICheckListApiService _checkListApiService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<DuplicatePunchItemCommandHandler> _logger;

    public DuplicatePunchItemCommandHandler(IPunchItemRepository punchItemRepository,
        IUnitOfWork unitOfWork,
        ISyncToPCS4Service syncToPCS4Service,
        IMessageProducer messageProducer,
        ICheckListApiService checkListApiService,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        ILogger<DuplicatePunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _syncToPCS4Service = syncToPCS4Service;
        _checkListApiService = checkListApiService;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(DuplicatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);
        var attachments = await _attachmentRepository.GetAttachmentsByParentGuid(request.PunchItemGuid, cancellationToken);
        
        var copies = request.CheckListGuids.Select(punchItem.ShallowCopy).ToList();

        var integrationEvents = new List<PunchItemCreatedIntegrationEvent>();
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var copy in copies)
            {
                _punchItemRepository.Add(copy);

                var isNull = copy.Priority == null;
                // must save twice when creating. Must save before publishing events both to set with internal database ID
                // since ItemNo depend on it. Must save after publishing events because we use outbox pattern
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            
                //expand based on all values in punch
                var properties = GetRequiredProperties(punchItem);
            
                //send history message: Duplicated from Guid, with attachments
                properties.Insert(0, new Property(nameof(PunchItem.ItemNo), punchItem.ItemNo, ValueDisplayType.IntAsText));
                //TODO expand properies with lib items etc

                var integrationEvent = await PublishPunchItemCreatedIntegrationEventsAsync(punchItem, properties, cancellationToken);
                integrationEvents.Add(integrationEvent);
                await _messageProducer.PublishAsync(integrationEvent, cancellationToken);
            
                // copy attachments and publish attachment copied event this method should not commit.
                await _attachmentService.CopyAttachments(attachments, typeof(PunchItem).Name, copy.Guid, punchItem.Project.Name,
                    cancellationToken);
            
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred on insertion of PunchListItem");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        
      
        
        //TODO Sync to PCS 4 og recalc

        return new SuccessResult<string>("Ok");
    }

    private List<IProperty> GetRequiredProperties(PunchItem punchItem)
        =>
        [
            new Property(nameof(PunchItem.Category), punchItem.Category.ToString()),
            new Property(nameof(PunchItem.Description), punchItem.Description),
            new Property(nameof(PunchItem.RaisedByOrg), punchItem.RaisedByOrg.Code),
            new Property(nameof(PunchItem.ClearingByOrg), punchItem.ClearingByOrg.Code)
        ];

    private async Task<PunchItemCreatedIntegrationEvent> PublishPunchItemCreatedIntegrationEventsAsync(
        PunchItem punchItem,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemCreatedIntegrationEvent(punchItem);
        await _messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryCreatedIntegrationEvent(
            $"Punch item {punchItem.Category} {punchItem.ItemNo} created",
            punchItem.Guid,
            punchItem.CheckListGuid,
            new User(punchItem.CreatedBy.Guid, punchItem.CreatedBy.GetFullName()),
            punchItem.CreatedAtUtc,
            properties);
        await _messageProducer.SendHistoryAsync(historyEvent, cancellationToken);

        return integrationEvent;
    }
}

