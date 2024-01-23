using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.Links;

public interface ILinkService
{
    Task<IEnumerable<LinkDto>> GetAllForParentAsync(
        Guid parentGuid,
        CancellationToken cancellationToken);
}
