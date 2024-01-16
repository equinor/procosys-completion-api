using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class MailTemplateRepository : EntityRepository<MailTemplate>, IMailTemplateRepository
{
    public MailTemplateRepository(CompletionContext context)
        : base(context, context.MailTemplates)
    {
    }
}
