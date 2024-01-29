using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public interface ICommentService
{
    Task<CommentDto> AddAndSaveAsync(
        IUnitOfWork unitOfWork,
        IHaveGuid parentEntity,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions,
        /*
         * The emailTemplateCode represent a code matching a MailTemplate in DB
         * This MailTemplate must have placeholders which match the built email context.
         * See ICompletionMailService for sample
         */
        string emailTemplateCode,
        CancellationToken cancellationToken);
    
    Guid Add(
        IHaveGuid parentEntity,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions);
}
