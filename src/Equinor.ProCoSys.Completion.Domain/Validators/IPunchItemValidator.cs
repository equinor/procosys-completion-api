using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface IPunchItemValidator
{
    Task<bool> ExistsAsync(Guid punchItemGuid, CancellationToken cancellationToken);
    bool CurrentUserIsVerifier(PunchItem punchItem);
}
