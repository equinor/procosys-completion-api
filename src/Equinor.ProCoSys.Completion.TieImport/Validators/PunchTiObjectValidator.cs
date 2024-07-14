using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Validators;

public static class PunchTiObjectValidator
{
    public static IEnumerable<ImportError> Validate(TIObject tiObject)
    {
        var project = tiObject.GetAttributeValueAsString(PunchObjectAttributes.Project);

        if (string.IsNullOrEmpty(project))
        {
            yield return tiObject.ToImportError("This punch item object is missing a project");
        }

        var tagNo = tiObject.GetAttributeValueAsString(PunchObjectAttributes.TagNo);
        if (string.IsNullOrEmpty(tagNo))
        {
            yield return tiObject.ToImportError("This punch item object is missng a tag number");
        }
    }
}


public static class TiObjectExtensions {
    public static ImportError ToImportError(this TIObject tiObject, string message)
    {
        var importError = new ImportError(
                tiObject.Guid,
                tiObject.Method,
            tiObject.GetAttributeValueAsString(PunchObjectAttributes.Project) ?? "N/A",
                tiObject.Site,
                message
            );

        return importError;
    }
}
