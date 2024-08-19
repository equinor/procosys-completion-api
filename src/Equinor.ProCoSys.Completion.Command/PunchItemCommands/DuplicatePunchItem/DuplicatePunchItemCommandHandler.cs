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

        // _attachmentRepository.GetAllAsync(cancellationToken)

        var copies = request.CheckListGuids.Select(punchItem.ShallowCopy).ToList();

        foreach (var copy in copies)
        {
            //await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                //  var properties = GetRequiredProperties(punchItem);
                _punchItemRepository.Add(copy);
                //    await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Add property for ItemNo first in list, since it is an "important" property
                //    properties.Insert(0,
                //         new Property(nameof(PunchItem.ItemNo), punchItem.ItemNo, ValueDisplayType.IntAsText));

                //   var integrationEvent =
                //       await PublishPunchItemCreatedIntegrationEventsAsync(punchItem, properties, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var res = await _attachmentService.CopyAttachments(attachments, typeof(PunchItem).Name, copy.Guid, punchItem.Project.Name,
                    cancellationToken);

                //  await _syncToPCS4Service.SyncNewPunchListItemAsync(integrationEvent, cancellationToken);

                //  await _checkListApiService.RecalculateCheckListStatus(punchItem.Plant, punchItem.CheckListGuid,
                //      cancellationToken);

                //       await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred on insertion of PunchListItem");
                //           await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

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

