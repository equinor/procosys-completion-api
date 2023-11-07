using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
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
                    join createdByUser in _context.QuerySet<Person>()
                        on c.CreatedById equals createdByUser.Id
                   where c.ParentGuid == parentGuid
                   select new CommentDto(
                       c.ParentGuid,
                       c.Guid,
                       c.Text,
                       new PersonDto(
                           createdByUser.Guid,
                           createdByUser.FirstName,
                           createdByUser.LastName,
                           createdByUser.UserName,
                           createdByUser.Email),
                       c.CreatedAtUtc
               ))
                .TagWith($"{nameof(CommentService)}.{nameof(GetAllForParentAsync)}")
                .ToListAsync(cancellationToken);

        return comments;
    }
}
