using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Statoil.TI.InterfaceServices.Message;
using static Equinor.ProCoSys.Completion.Domain.Imports.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Validators;

public static class PunchTiObjectValidator
{
    public static ImportResult Validate(TIObject tiObject)
    {
        string[] requiredStringAttributes = [Project, TagNo, ExternalPunchItemNo, FormType];

        var errors = requiredStringAttributes
            .Where(attributeName => string.IsNullOrEmpty(tiObject.GetAttributeValueAsString(attributeName)))
            .Select(attributeName =>
                tiObject.ToImportError($"This punch item object is missing required attribute '{attributeName}'"));

        return new ImportResult(tiObject, null, default, errors.ToArray());
    }
}
