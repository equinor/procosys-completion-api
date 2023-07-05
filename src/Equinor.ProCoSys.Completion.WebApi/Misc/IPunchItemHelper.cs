using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public interface IPunchItemHelper
{
    Task<Guid?> GetProjectGuidForPunchItemAsync(Guid punchItemGuid);
}
