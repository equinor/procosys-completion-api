using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Validators;

public static class PunchTiObjectValidator
{
    public static IEnumerable<ImportError> Validate(TIObject tiObject)
    {
        var project = tiObject.GetAttributeValueAsString("PROJECT");

        if (project is null)
        {
            yield return tiObject.ToImportError("This punch item object is missing a project");
        }
    }
}


public static class TiObjectExtensions {
    public static ImportError ToImportError(this TIObject tiObject, string message)
    {
        var importError = new ImportError(
                tiObject.Guid,
                tiObject.Method,
            tiObject.GetAttributeValueAsString("PROJECT") ?? "N/A",
                tiObject.Site,
                message
            );

        return importError;
    }
}
