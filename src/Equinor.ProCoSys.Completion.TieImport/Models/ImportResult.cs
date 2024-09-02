using Equinor.ProCoSys.Completion.Domain.Imports;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Models;

public readonly record struct ImportResult(
    TIObject TiObject,
    PunchItemImportMessage? Message,
    IEnumerable<ImportError> Errors)
{
    public ImportError GetImportError(string message) =>
        new(TiObject.Guid, TiObject.Method, TiObject.Project, TiObject.Site, message);
};
