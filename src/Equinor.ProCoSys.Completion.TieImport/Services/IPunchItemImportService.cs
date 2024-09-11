using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.Services;

public interface IPunchItemImportService
{
    /// <summary>
    /// Tries to import punchItem and returns list of errors if not successful. Empty list if successful.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<List<ImportError>> HandlePunchImportMessageAsync(PunchItemImportMessage message);
}
