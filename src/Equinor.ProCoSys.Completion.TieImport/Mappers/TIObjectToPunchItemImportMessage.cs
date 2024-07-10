using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public static class TiObjectToPunchItemImportMessage
{
    public static PunchItemImportMessage ToPunchItemImportMessage(TIObject tiObject)
    {
        var foo = new PunchItemImportMessage(
            new Optional<string?>(),
            new Optional<string?>(),
            new Optional<string?>(),
            new Optional<string?>(),
            new Optional<string?>()
        );

        return default;
    }

    private static Optional<string?> GetStringValue(TIObject tiObject, string attribute) =>
        tiObject.Attributes.Any(a => a.Name == attribute)
            ? new Optional<string?>(tiObject.GetAttributeValueAsString(attribute))
            : new Optional<string?>();
}
