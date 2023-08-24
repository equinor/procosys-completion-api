using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;
public static class TIObjectExtensions
{
    public static string GetLogFriendlyString(this TIBaseObject tiObject)
    {
        //Make Class.Classfication.Name string for the logs.
        var logPrefix = $"{tiObject.Site}.{tiObject.ObjectClass}";
        logPrefix += ExtendedLogPrefix(tiObject);

        return $"{logPrefix} ";
    }
    public static string GetLogFriendlyStringForTieLog(this TIBaseObject tiObject)
    {
        //Make Class.Classfication.Name string for the logs.
        var logPrefix = $"{tiObject.ObjectClass}";
        logPrefix += ExtendedLogPrefix(tiObject);

        return $"{logPrefix} ";
    }

    private static string ExtendedLogPrefix(TIBaseObject tiObject)
    {
        var logPrefix = string.Empty;

        if (!string.IsNullOrWhiteSpace(tiObject.Classification) &&
            tiObject.ObjectClass != tiObject.Classification)
        {
            logPrefix += $".{tiObject.Classification}";
        }

        if (!string.IsNullOrWhiteSpace(tiObject.ObjectName))
        {
            logPrefix += $".{tiObject.ObjectName}";
        }
        else if (tiObject.GetAttributeValue("Name") != null)
        {
            logPrefix += $".{tiObject.GetAttributeValue("Name")}";
        }
        return $"{logPrefix} ";
    }
}
