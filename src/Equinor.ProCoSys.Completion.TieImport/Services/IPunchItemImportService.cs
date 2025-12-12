using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;
using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.Services;

public interface IPunchItemImportService
{
    /// <summary>
    /// Imports a mapped TIObject of type PUNCHITEM to ProCoSys
    /// </summary>
    /// <param name="message">PunchItemImportMessage with Method (CREATE, UPDATE or APPEND)</param>
    /// <returns>List of import errors, empty if successful</returns>
    Task<List<ImportError>> HandlePunchImportMessageAsync(PunchItemImportMessage message);
}
