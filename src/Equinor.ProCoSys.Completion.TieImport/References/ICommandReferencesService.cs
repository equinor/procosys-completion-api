using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.References;

public interface ICommandReferencesService
{
    Task<CommandReferences> GetAndValidatePunchItemReferencesForImportAsync(
        PunchItemImportMessage message,
        CancellationToken cancellationToken);
}
