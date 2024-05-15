using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;

public interface IMailTemplateRepository : IRepository<MailTemplate>
{
    Task<MailTemplate> GetNonVoidedByCodeAsync(string plant, string code, CancellationToken cancellationToken);
}
