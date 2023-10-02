using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command;

public static class ListOfIPropertyExtension
{
    public static void AddChangeIfNotNull(this List<IProperty> changes, IProperty? change)
    {
        if (change is not null)
        {
            changes.Add(change);
        }
    }
}
