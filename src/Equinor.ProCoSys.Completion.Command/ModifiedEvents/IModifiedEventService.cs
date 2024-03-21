using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.ModifiedEvents;

public interface IModifiedEventService
{
    Task<ModifiedEvent> GetModifiedEventAsync(CancellationToken cancellationToken);
}
