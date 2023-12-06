using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.Comments;

public class CommentService : ICommentService
{
    private readonly IReadOnlyContext _context;

    public CommentService(IReadOnlyContext context) => _context = context;

    public async Task<IEnumerable<CommentDto>> GetAllForParentAsync(
        Guid parentGuid,
        CancellationToken cancellationToken)
    {
        var comments =
            await (from c in _context.QuerySet<Comment>()
                        .Include(a => a.Labels.Where(l => !l.IsVoided))
                        .Include(a => a.CreatedBy)
                    where c.ParentGuid == parentGuid
                    select c)
                .TagWith($"{nameof(CommentService)}.{nameof(GetAllForParentAsync)}")
                .ToListAsync(cancellationToken);

        var commentsDtos = comments.Select(c => new CommentDto(
            c.ParentGuid,
            c.Guid,
            c.Text,
            c.GetOrderedNonVoidedLabels().Select(l => l.Text).ToList(),
            new PersonDto(
                c.CreatedBy.Guid,
                c.CreatedBy.FirstName,
                c.CreatedBy.LastName,
                c.CreatedBy.UserName,
                c.CreatedBy.Email),
            c.CreatedAtUtc));
        
        return commentsDtos;
    }
}
