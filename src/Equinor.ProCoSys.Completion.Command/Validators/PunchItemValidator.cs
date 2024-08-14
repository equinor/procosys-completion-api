using System.Threading.Tasks;
using System.Threading;
using System;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class PunchItemValidator(IReadOnlyContext context, ICurrentUserProvider currentUserProvider) : IPunchItemValidator
{
    public async Task<bool> ExistsAsync(Guid punchItemGuid, CancellationToken cancellationToken)
        => await context.QuerySet<PunchItem>()
            .TagWith($"{nameof(PunchItemValidator)}.{nameof(ExistsAsync)}")
            .AnyAsync(p => p.Guid == punchItemGuid, cancellationToken);

    public bool CurrentUserIsVerifier(PunchItem punchItem)
    {
        var currentUserOid = currentUserProvider.GetCurrentUserOid();
        return punchItem.VerifiedBy is not null && punchItem.VerifiedBy.Guid == currentUserOid;
    }
}
