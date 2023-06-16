using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public interface IPunchHelper
{
    Task<string?> GetProjectNameAsync(Guid punchGuid);
}
