using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Command.EventPublishers.HistoryEvents;
using Equinor.ProCoSys.Completion.Command.EventPublishers.PunchItemEvents;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommandHandler : IRequestHandler<RejectPunchItemCommand, Result<string>>
{
    public const string RejectReasonPropertyName = "Reject reason";

    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ILabelRepository _labelRepository;
    private readonly ICommentService _commentService;
    private readonly IPersonRepository _personRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPunchEventPublisher _punchEventPublisher;
    private readonly IHistoryEventPublisher _historyEventPublisher;
    private readonly ILogger<RejectPunchItemCommandHandler> _logger;
    private readonly string _rejectLabelText;

    public RejectPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ILabelRepository labelRepository,
        ICommentService commentService,
        IPersonRepository personRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IPunchEventPublisher punchEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
        ILogger<RejectPunchItemCommandHandler> logger,
        IOptionsMonitor<ApplicationOptions> options)
    {
        _punchItemRepository = punchItemRepository;
        _labelRepository = labelRepository;
        _commentService = commentService;
        _personRepository = personRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _punchEventPublisher = punchEventPublisher;
        _historyEventPublisher = historyEventPublisher;
        _logger = logger;
        _rejectLabelText = options.CurrentValue.RejectLabel;
    }

    public async Task<Result<string>> Handle(RejectPunchItemCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var rejectLabel = await _labelRepository.GetByTextAsync(_rejectLabelText, cancellationToken);
            var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

            var change = await RejectAsync(punchItem, request.Comment, cancellationToken);

            var mentions = await _personRepository.GetOrCreateManyAsync(request.Mentions, cancellationToken);

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await _punchEventPublisher.PublishUpdatedEventAsync(punchItem, cancellationToken);
            await _historyEventPublisher.PublishUpdatedEventAsync(
                punchItem.Plant,
                "Punch item rejected",
                punchItem.Guid,
                new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
                punchItem.ModifiedAtUtc!.Value,
                [change],
                cancellationToken);

            _commentService.Add(nameof(PunchItem), request.PunchItemGuid, request.Comment, [rejectLabel], mentions);

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncObjectUpdateAsync("PunchItem", integrationEvent, punchItem.Plant, cancellationToken);

            // todo 109356 add unit tests
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} rejected", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on update of punch item with guid {PunchItemGuid}.", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<IProperty> RejectAsync(
        PunchItem punchItem,
        string comment,
        CancellationToken cancellationToken)
    {
        var currentPerson = await _personRepository.GetCurrentPersonAsync(cancellationToken);
        var change = new Property<string?>(RejectReasonPropertyName, null, comment);
        punchItem.Reject(currentPerson);

        return change;
    }
}
