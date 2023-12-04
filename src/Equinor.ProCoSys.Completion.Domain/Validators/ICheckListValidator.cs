using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface ICheckListValidator
{
    Task<bool> ExistsAsync(Guid checkListGuid);
    Task<bool> TagOwningCheckListIsVoidedAsync(Guid checkListGuid);
    Task<bool> InProjectAsync(Guid checkListGuid, Guid projectGuid);
}
