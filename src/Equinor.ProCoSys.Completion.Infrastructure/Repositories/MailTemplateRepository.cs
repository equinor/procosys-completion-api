using System.Linq;
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

    public async Task<MailTemplate> GetNonVoidedByCodeAsync(string plant, string code, CancellationToken cancellationToken)
    {
        var mailTemplates = await DefaultQueryable
            .Where(mt => !mt.IsVoided && mt.Code == code && (mt.Plant == plant || mt.Plant == null))
            .ToListAsync(cancellationToken);
        var mailTemplate = mailTemplates.SingleOrDefault(mt => mt.Plant == plant) ?? 
                           mailTemplates.SingleOrDefault(mt => mt.Plant == null);
        if (mailTemplate is null)
        {
            throw new EntityNotFoundException($"Could not find non voided{nameof(MailTemplate)} with code {code}. Must be configured ...");
        }
        return mailTemplate;
    }
}
