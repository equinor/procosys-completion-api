using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Links;

public interface ILinkService
{
    Task<LinkDto> AddAsync(
        string sourceType,
        Guid sourceGuid,
        string title,
        string url,
        CancellationToken cancellationToken);
    
    Task<string> UpdateAsync(
        Guid guid,
        string title,
        string url,
        string rowVersion,
        CancellationToken cancellationToken);
    
    Task DeleteAsync(
        Guid guid,
        string rowVersion,
        CancellationToken cancellationToken);
    
    Task<bool> ExistsAsync(Guid guid);
}
