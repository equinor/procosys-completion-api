using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommandHandler : PunchUpdateCommandBase, IRequestHandler<RejectPunchItemCommand, Result<string>>
{
    public const string RejectReasonPropertyName = "Reject reason";

    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ILabelRepository _labelRepository;
    private readonly ICommentService _commentService;
    private readonly IPersonRepository _personRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly ICompletionMailService _completionMailService;
    private readonly IDeepLinkUtility _deepLinkUtility;
    private readonly IMessageProducer _messageProducer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICheckListApiService _checkListApiService;
    private readonly ILogger<RejectPunchItemCommandHandler> _logger;
    private readonly IOptionsMonitor<ApplicationOptions> _options;

    public RejectPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ILabelRepository labelRepository,
        ICommentService commentService,
        IPersonRepository personRepository,
        ISyncToPCS4Service syncToPCS4Service,
        ICompletionMailService completionMailService,
        IDeepLinkUtility deepLinkUtility,
        IMessageProducer messageProducer,
        IUnitOfWork unitOfWork,
        ICheckListApiService checkListApiService,
        ILogger<RejectPunchItemCommandHandler> logger,
        IOptionsMonitor<ApplicationOptions> options)
    {
        _punchItemRepository = punchItemRepository;
        _labelRepository = labelRepository;
        _commentService = commentService;
        _personRepository = personRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _completionMailService = completionMailService;
        _deepLinkUtility = deepLinkUtility;
        _messageProducer = messageProducer;
        _unitOfWork = unitOfWork;
        _checkListApiService = checkListApiService;
        _logger = logger;
        _options = options;
    }

    public async Task<Result<string>> Handle(RejectPunchItemCommand request, CancellationToken cancellationToken)
    {
        var rejectLabel = await _labelRepository.GetByTextAsync(_options.CurrentValue.RejectLabel, cancellationToken);
        var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

        var change = await RejectAsync(punchItem, request.Comment, cancellationToken);

        var mentions = await _personRepository.GetOrCreateManyAsync(request.Mentions, cancellationToken);
        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
           // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
                _messageProducer,
                punchItem,
                "Punch item rejected",
                [change],
                cancellationToken);

            await _commentService.AddAsync(punchItem, punchItem.Plant, request.Comment, [rejectLabel], mentions, cancellationToken);

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            await _checkListApiService.RecalculateCheckListStatus(punchItem.Plant, punchItem.CheckListGuid, cancellationToken);

            await SendEMailAsync(punchItem, request.Comment, mentions, cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} rejected", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred on reject of PunchListItem with guid {PunchItemGuid}", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task SendEMailAsync(PunchItem punchItem, string comment, List<Person> mentions, CancellationToken cancellationToken)
    {
        var emailContext = GetEmailContext(punchItem, comment);
        var emailAddresses = mentions.Select(m => m.Email).ToList();

        await _completionMailService.SendEmailAsync(MailTemplateCode.PunchRejected, emailContext, emailAddresses, cancellationToken);
    }

    private dynamic GetEmailContext(PunchItem punchItem, string comment)
    {
        var emailContext = punchItem.GetEmailContext();
        
        emailContext.Comment = comment;
        emailContext.Url = _deepLinkUtility.CreateUrl(punchItem.GetContextName(), punchItem.Guid);

        return emailContext;
    }

    private async Task<IChangedProperty> RejectAsync(
        PunchItem punchItem,
        string comment,
        CancellationToken cancellationToken)
    {
        var currentPerson = await _personRepository.GetCurrentPersonAsync(cancellationToken);
        var change = new ChangedProperty<string?>(RejectReasonPropertyName, null, comment);
        punchItem.Reject(currentPerson);

        return change;
    }
}
