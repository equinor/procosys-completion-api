using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;

public interface IMailTemplateRepository : IRepository<MailTemplate>
{
    Task<MailTemplate> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
