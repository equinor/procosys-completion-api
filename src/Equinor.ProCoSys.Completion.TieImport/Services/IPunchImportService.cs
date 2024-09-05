using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.Services;

public interface IPunchImportService
{
    Task<ImportResult> HandlePunchImportMessage(PunchItemImportMessage message);
}
