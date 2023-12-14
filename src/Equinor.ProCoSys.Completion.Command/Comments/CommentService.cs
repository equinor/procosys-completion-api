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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CommentDto> AddAsync(
        string parentType,
        Guid parentGuid,
        string text,
        IEnumerable<Label> labels,
        CancellationToken cancellationToken)
    {
        var comment = new Comment(parentType, parentGuid, text);
        comment.UpdateLabels(labels.ToList());

        _commentRepository.Add(comment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Comment with guid {CommentGuid} created for {Type} : {CommentParentGuid}", 
            comment.Guid, 
            comment.ParentType,
            comment.ParentGuid);

        return new CommentDto(comment.Guid, comment.RowVersion.ConvertToString());
    }

    public async Task<CommentDto> AddAsync(
        string parentType,
        Guid parentGuid,
        string text,
        Label label,
        CancellationToken cancellationToken)
        => await AddAsync(parentType, parentGuid, text, new List<Label> { label }, cancellationToken);
}
