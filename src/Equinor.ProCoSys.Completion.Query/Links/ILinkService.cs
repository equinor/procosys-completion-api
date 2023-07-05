using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.Links;

public interface ILinkService
{
    Task<IEnumerable<LinkDto>> GetAllForSourceAsync(
        Guid sourceGuid,
        CancellationToken cancellationToken);
}
