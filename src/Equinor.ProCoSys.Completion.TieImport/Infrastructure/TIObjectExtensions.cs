using Equinor.ProCoSys.Completion.TieImport.Extensions;
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

    public static bool IsClass(this TIBaseObject tiObject, params string[] classes)
        => tiObject?.ObjectClass != null &&
           classes.Any(c => tiObject.ObjectClass.Equals(c, StringComparison.InvariantCultureIgnoreCase));

    private static bool IsMethod(TIBaseObject tiObject, params string[] methods)
        => methods.Any(m => tiObject.Method.Equals(m, StringComparison.InvariantCultureIgnoreCase));

    public static bool IsMethodCreate(this TIBaseObject tiObject)
        => IsMethod(tiObject, "Create", "Insert", "Allocate");

    public static bool IsMethodUpdate(this TIBaseObject tiObject) => IsMethod(tiObject, "Update");

    public static bool IsMethodModify(this TIBaseObject tiObject) =>
        !tiObject.IsMethodCreate() && !tiObject.IsMethodUpdate() && !tiObject.IsMethodDelete();

    public static bool IsMethodDelete(this TIBaseObject tiObject) => IsMethod(tiObject, "Delete");

    public static TIAttribute? GetAttributeCaseInsensitive(this TIBaseObject tiObject, string attributeName)
        => tiObject?.Attributes == null || string.IsNullOrWhiteSpace(attributeName)
            ? null
            : tiObject
                .Attributes
                .FirstOrDefault(a => a.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase));

    public static string? GetAttributeValueAsString(this TIBaseObject tiObject, string attributeName) =>
        tiObject.GetAttributeCaseInsensitive(attributeName)!.GetValueAsString();

    public static string? GetAttributeValueAsStringUpperCase(this TIBaseObject tiObject, string attributeName)
        => tiObject.GetAttributeCaseInsensitive(attributeName)!.GetValueAsStringUpperCase();

    public static DateTime? GetAttributeValueAsDateTime(this TIBaseObject tiObject, string attributeName)
        => tiObject.GetAttributeCaseInsensitive(attributeName)!.GetValueAsDateTime();

    public static bool? GetAttributeValueAsBool(this TIBaseObject tiObject, string attributeName)
        => tiObject.GetAttributeCaseInsensitive(attributeName)!.GetValueAsBool();

    public static double? GetAttributeValueAsDouble(this TIBaseObject tiObject, string attributeName)
        => tiObject.GetAttributeCaseInsensitive(attributeName)!.GetValueAsDouble();

    public static bool HasAttributeValue(this TIBaseObject tiObject, string attributeName)
        => tiObject.GetAttributeCaseInsensitive(attributeName)!.HasValue();

    public static bool HasAttributeValueBlankingSignal(this TIBaseObject tiObject, string attributeName)
        => tiObject.GetAttributeCaseInsensitive(attributeName)!.HasBlankingSignal();

    public static bool
        AttributeValueIsDifferentFrom(this TIBaseObject tiObject, string attributeName, string attributeValue)
        => HasAttributeValue(tiObject, attributeName) && !GetAttributeValueAsString(tiObject, attributeName)!
               .Equals(attributeValue, StringComparison.InvariantCulture);

    public static TIRelationship? GetRelationship(this TIObject tiObject, string relationshipName)
        => tiObject.Relationships == null || string.IsNullOrWhiteSpace(relationshipName)
            ? null
            : tiObject
                .Relationships
                .FirstOrDefault(a => a.Name.Equals(relationshipName, StringComparison.InvariantCultureIgnoreCase));

    public static TISubObject? GetSubObject(this TIObject tiObject, string subObjectClass)
        => tiObject.SubObjects == null || string.IsNullOrWhiteSpace(subObjectClass)
            ? null
            : tiObject
                .SubObjects
                .FirstOrDefault(a =>
                    a.ObjectClass.Equals(subObjectClass, StringComparison.InvariantCultureIgnoreCase));
}
