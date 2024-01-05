using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectPunchItemCommandHandler> _logger;
    private readonly string _rejectLabelText;

    public RejectPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ILabelRepository labelRepository,
        ICommentService commentService,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        ILogger<RejectPunchItemCommandHandler> logger,
        IOptionsMonitor<ApplicationOptions> options)
    {
        _punchItemRepository = punchItemRepository;
        _labelRepository = labelRepository;
        _commentService = commentService;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _rejectLabelText = options.CurrentValue.RejectLabel;
    }

    public async Task<Result<string>> Handle(RejectPunchItemCommand request, CancellationToken cancellationToken)
    {
        var rejectLabel = await _labelRepository.GetByTextAsync(_rejectLabelText, cancellationToken);
        var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

        var currentPerson = await _personRepository.GetCurrentPersonAsync(cancellationToken);
        punchItem.Reject(currentPerson);
        punchItem.SetRowVersion(request.RowVersion);

        var mentions = await _personRepository.GetOrCreateManyAsync(request.Mentions, cancellationToken);

        await _commentService.AddAsync(
            nameof(PunchItem),
            request.PunchItemGuid,
            request.Comment,
            rejectLabel,
            mentions,
            cancellationToken);

        punchItem.AddDomainEvent(new PunchItemRejectedDomainEvent(
            punchItem,
            new List<IProperty> { new Property<string?>(RejectReasonPropertyName, null, request.Comment) }));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} rejected", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }
}
