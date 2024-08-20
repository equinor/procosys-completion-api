using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface ICheckListValidator
{
    Task<bool> TagOwningCheckListIsVoidedAsync(Guid checkListGuid, CancellationToken cancellationToken);
}
