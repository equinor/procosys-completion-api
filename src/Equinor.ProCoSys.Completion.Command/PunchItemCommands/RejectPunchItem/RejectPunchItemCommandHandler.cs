using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
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
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectPunchItemCommandHandler> _logger;
    private readonly IOptionsMonitor<ApplicationOptions> _options;

    public RejectPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ILabelRepository labelRepository,
        ICommentService commentService,
        IPersonRepository personRepository,
        ISyncToPCS4Service syncToPCS4Service,
        ICompletionMailService completionMailService,
        IIntegrationEventPublisher integrationEventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<RejectPunchItemCommandHandler> logger,
        IOptionsMonitor<ApplicationOptions> options)
    {
        _punchItemRepository = punchItemRepository;
        _labelRepository = labelRepository;
        _commentService = commentService;
        _personRepository = personRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _completionMailService = completionMailService;
        _integrationEventPublisher = integrationEventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _options = options;
    }

    public async Task<Result<string>> Handle(RejectPunchItemCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var rejectLabel = await _labelRepository.GetByTextAsync(_options.CurrentValue.RejectLabel, cancellationToken);
            var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

            var change = await RejectAsync(punchItem, request.Comment, cancellationToken);

            var mentions = await _personRepository.GetOrCreateManyAsync(request.Mentions, cancellationToken);

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
                _integrationEventPublisher,
                punchItem,
                "Punch item rejected",
                [change],
                cancellationToken);

            _commentService.Add(nameof(PunchItem), request.PunchItemGuid, request.Comment, [rejectLabel], mentions);

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncObjectUpdateAsync(SyncToPCS4Service.PunchItem, integrationEvent, punchItem.Plant, cancellationToken);

            await SendEMailAsync(punchItem, request.Comment, mentions, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} rejected", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on reject of punch item with guid {PunchItemGuid}.", request.PunchItemGuid);
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
        dynamic emailContext = new ExpandoObject();
        
        emailContext.PunchItem = punchItem;
        emailContext.Comment = comment;
        // todo 109830 Deep link to the punch item
        emailContext.Url = _options.CurrentValue.BaseUrl;

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
