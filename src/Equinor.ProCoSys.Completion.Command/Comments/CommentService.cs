using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICompletionMailService _completionMailService;
    private readonly IDeepLinkUtility _deepLinkUtility;
    private readonly IPersonRepository _personRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        ICommentRepository commentRepository,
        ICompletionMailService completionMailService,
        IDeepLinkUtility deepLinkUtility,
        IPersonRepository personRepository,
        ISyncToPCS4Service syncToPCS4Service,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _completionMailService = completionMailService;
        _deepLinkUtility = deepLinkUtility;
        _personRepository = personRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _logger = logger;
    }

    public async Task<CommentDto> AddAndSaveAsync(
        IUnitOfWork unitOfWork,
        IHaveGuid parentEntity,
        string plant,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions,
        string emailTemplateCode,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var comment = AddToRepository(parentEntity, text, labels, mentions);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var currentPerson = await _personRepository.GetCurrentPersonAsync(cancellationToken);
            var createdBy = new User(currentPerson.Guid, currentPerson.GetFullName());

            var commentEvent = new CommentEventDto
            {
                Guid = comment.Guid,
                Plant = plant,
                ParentGuid = comment.ParentGuid,
                CreatedBy = createdBy,
                CreatedAtUtc = TimeService.UtcNow,
                Text = comment.Text,
                Labels = comment.Labels.Select(x => x.Text).ToList()
            };

            await _syncToPCS4Service.SyncNewCommentAsync(commentEvent, cancellationToken);

            await SendEMailAsync(emailTemplateCode, parentEntity, comment, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Comment with guid {CommentGuid} created for {Type} : {CommentParentGuid}",
                comment.Guid,
                comment.ParentType,
                comment.ParentGuid);

            return new CommentDto(comment.Guid, comment.RowVersion.ConvertToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred on insertion of Comment (with saving)");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Guid> AddAsync(IHaveGuid parentEntity, string plant, string text, IEnumerable<Label> labels, IEnumerable<Person> mentions, CancellationToken cancellationToken)
    {
        try
        {
            var comment = AddToRepository(parentEntity, text, labels, mentions);

            var currentPerson = await _personRepository.GetCurrentPersonAsync(cancellationToken);
            var createdBy = new User(currentPerson.Guid, currentPerson.GetFullName());

            var commentEvent = new CommentEventDto
            {
                Guid = comment.Guid,
                Plant = plant,
                ParentGuid = comment.ParentGuid,
                CreatedBy = createdBy,
                CreatedAtUtc = TimeService.UtcNow,
                Text = comment.Text,
                Labels = comment.Labels.Select(x => x.Text).ToList()
            };

            await _syncToPCS4Service.SyncNewCommentAsync(commentEvent, cancellationToken);

            _logger.LogInformation("Comment with guid {CommentGuid} created for {Type} : {CommentParentGuid}",
                comment.Guid,
                comment.ParentType,
                comment.ParentGuid);

            return comment.Guid;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred on insertion of Comment (without saving)");
            throw;
        }
    }

    private Comment AddToRepository(
        IHaveGuid parentEntity,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions)
    {
        var comment = new Comment(parentEntity.GetContextName(), parentEntity.Guid, text);
        comment.UpdateLabels(labels.ToList());
        comment.SetMentions(mentions.ToList());

        _commentRepository.Add(comment);

        return comment;
    }

    private async Task SendEMailAsync(
        string emailTemplateCode,
        IHaveGuid parentEntity,
        Comment comment, 
        CancellationToken cancellationToken)
    {
        var emailContext = parentEntity.GetEmailContext();
        emailContext.Comment = comment;
        emailContext.Url = _deepLinkUtility.CreateUrl(parentEntity.GetContextName(), parentEntity.Guid);
        
        var emailAddresses = comment.Mentions.Select(m => m.Email).ToList();
        await _completionMailService.SendEmailAsync(emailTemplateCode, emailContext, emailAddresses, cancellationToken);
    }
}
