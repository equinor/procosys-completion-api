﻿using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Extensions;

public static class TIObjectExtensions
{
    public static string? GetAttributeValueAsString(this TIBaseObject tiObject, string attributeName) =>
        tiObject.GetAttributeCaseInsensitive(attributeName)?.GetValueAsString();

    private static TIAttribute? GetAttributeCaseInsensitive(this TIBaseObject tiObject, string attributeName)
        => tiObject.Attributes?.SingleOrDefault(a => a.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase));
}