using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICompletionMailService _completionMailService;
    private readonly IDeepLinkUtility _deepLinkUtility;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        ICommentRepository commentRepository,
        ICompletionMailService completionMailService,
        IDeepLinkUtility deepLinkUtility,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _completionMailService = completionMailService;
        _deepLinkUtility = deepLinkUtility;
        _logger = logger;
    }

    public async Task<CommentDto> AddAndSaveAsync(
        IUnitOfWork unitOfWork,
        IEntityContext parentEntity,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions,
        string emailTemplateCode,
        CancellationToken cancellationToken)
    {
        var comment = AddToRepository(parentEntity, text, labels, mentions);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await SendEMailAsync(emailTemplateCode, parentEntity, comment, cancellationToken);

        _logger.LogInformation("Comment with guid {CommentGuid} created for {Type} : {CommentParentGuid}",
            comment.Guid,
            comment.ParentType,
            comment.ParentGuid);

        return new CommentDto(comment.Guid, comment.RowVersion.ConvertToString());
    }

    public Guid Add(IEntityContext parentEntity, string text, IEnumerable<Label> labels, IEnumerable<Person> mentions)
    {
        var comment = AddToRepository(parentEntity, text, labels, mentions);

        _logger.LogInformation("Comment with guid {CommentGuid} created for {Type} : {CommentParentGuid}",
            comment.Guid,
            comment.ParentType,
            comment.ParentGuid);

        return comment.Guid;
    }

    private Comment AddToRepository(
        IEntityContext parentEntity,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions)
    {
        var comment = new Comment(parentEntity.GetContextType(), parentEntity.Guid, text);
        comment.UpdateLabels(labels.ToList());
        comment.SetMentions(mentions.ToList());

        _commentRepository.Add(comment);

        return comment;
    }

    private async Task SendEMailAsync(
        string emailTemplateCode,
        IEntityContext parentEntity,
        Comment comment, 
        CancellationToken cancellationToken)
    {
        var emailContext = parentEntity.GetEmailContext();
        emailContext.Comment = comment;
        emailContext.Url = _deepLinkUtility.CreateUrl(parentEntity.GetContextType(), parentEntity.Guid);
        
        var emailAddresses = comment.Mentions.Select(m => m.Email).ToList();
        await _completionMailService.SendEmailAsync(emailTemplateCode, emailContext, emailAddresses, cancellationToken);
    }
}
