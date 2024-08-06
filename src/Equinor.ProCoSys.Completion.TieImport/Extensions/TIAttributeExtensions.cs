using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Extensions;

public static class TiAttributeExtensions
{
    public static string? GetValueAsString(this TIAttribute tiAttribute)
        => string.IsNullOrWhiteSpace(tiAttribute.Value) ? null : tiAttribute.Value.Trim();
}
