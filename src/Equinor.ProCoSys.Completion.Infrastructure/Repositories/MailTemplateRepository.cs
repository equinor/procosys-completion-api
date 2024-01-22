using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class MailTemplateRepository : EntityRepository<MailTemplate>, IMailTemplateRepository
{
    public MailTemplateRepository(CompletionContext context)
        : base(context, context.MailTemplates)
    {
    }

    // todo unit test
    public async Task<MailTemplate> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var mailTemplate = await DefaultQuery.SingleOrDefaultAsync(
            x => x.Code == code,
            cancellationToken);
        if (mailTemplate is null)
        {
            throw new EntityNotFoundException($"Could not find {nameof(MailTemplate)} with code {code}. Must be configured ...");
        }
        return mailTemplate;
    }
}
