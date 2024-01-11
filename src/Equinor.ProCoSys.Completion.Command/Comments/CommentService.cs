using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        ICommentRepository commentRepository,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<CommentDto> AddAndSaveAsync(
        IUnitOfWork unitOfWork,
        string parentType,
        Guid parentGuid,
        string text,
        IEnumerable<Label> labels,
        CancellationToken cancellationToken)
    {
        var comment = AddToRepository(parentType, parentGuid, text, labels);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Comment with guid {CommentGuid} created for {Type} : {CommentParentGuid}",
            comment.Guid,
            comment.ParentType,
            comment.ParentGuid);

        return new CommentDto(comment.Guid, comment.RowVersion.ConvertToString());
    }

    public Guid Add(string parentType, Guid parentGuid, string text, IEnumerable<Label> labels)
    {
        var comment = AddToRepository(parentType, parentGuid, text, labels);

        _logger.LogInformation("Comment with guid {CommentGuid} created for {Type} : {CommentParentGuid}",
            comment.Guid,
            comment.ParentType,
            comment.ParentGuid);

        return comment.Guid;
    }

    private Comment AddToRepository(
        string parentType,
        Guid parentGuid,
        string text,
        IEnumerable<Label> labels)
    {
        var comment = new Comment(parentType, parentGuid, text);
        comment.UpdateLabels(labels.ToList());

        _commentRepository.Add(comment);

        return comment;
    }
}
