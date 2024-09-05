using Equinor.ProCoSys.Completion.Domain.Imports;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Extensions;

public static class TiObjectExtensions
{
    public static string? GetAttributeValueAsString(this TIBaseObject tiObject, string attributeName) =>
        tiObject.GetAttributeCaseInsensitive(attributeName)?.GetValueAsString();

    private static TIAttribute? GetAttributeCaseInsensitive(this TIBaseObject tiObject, string attributeName)
        => tiObject.Attributes?.SingleOrDefault(a => a.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase));
    
    public static ImportError ToImportError(this TIObject tiObject, string message)
    {
        var importError = new ImportError(
            tiObject.Guid,
            tiObject.Method,
            tiObject.Project,
            tiObject.Site,
            message
        );

        return importError;
    }
}
