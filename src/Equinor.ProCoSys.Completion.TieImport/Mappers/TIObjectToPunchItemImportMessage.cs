using System.Globalization;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Statoil.TI.InterfaceServices.Message;
using static System.String;
using static Equinor.ProCoSys.Completion.TieImport.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public static class TiObjectToPunchItemImportMessage
{
    public static PunchItemImportMessage ToPunchItemImportMessage(TIObject tiObject)
    {
        var message = new PunchItemImportMessage(
            tiObject.Guid,
            tiObject.Method,
            tiObject.Project,
            tiObject.Site,
            GetStringValueOrThrow(tiObject, TagNo),
            GetStringValueOrThrow(tiObject, ExternalPunchItemNo),
            GetStringValueOrThrow(tiObject, FormType),
            GetStringValueOrThrow(tiObject, Responsible),
            GetLongValue(tiObject, PunchItemNo),
            GetStringValue(tiObject, Description),
            GetStringValue(tiObject, RaisedByOrganization),
            GetCategoryFromStatus(tiObject),
            GetStringValueWithNull(tiObject, PunchListType),
            GetDateValueWithNull(tiObject, DueDate),
            GetDateValue(tiObject, ClearedDate),
            GetStringValue(tiObject, ClearedBy),
            GetStringValue(tiObject, ClearedByOrganization),
            GetDateValue(tiObject, VerifiedDate),
            GetStringValue(tiObject, VerifiedBy),
            GetDateValue(tiObject, RejectedDate),
            GetStringValue(tiObject, RejectedBy),
            GetBoolValue(tiObject, MaterialRequired),
            GetDateValueWithNull(tiObject, MaterialEta),
            GetStringValueWithNull(tiObject, MaterialNo),
            GetStringValueWithNull(tiObject, ActionBy),
            GetStringValueWithNull(tiObject, DocumentNo),
            GetIntValueWithNull(tiObject, Estimate),
            GetStringValueWithNull(tiObject, OriginalWorkOrderNo),
            GetStringValueWithNull(tiObject, WorkOrderNo),
            GetStringValueWithNull(tiObject, Priority),
            GetStringValueWithNull(tiObject, Sorting),
            GetIntValueWithNull(tiObject, SwcrNo),
            GetStringValue(tiObject, IsVoided)
        );
        return message;
    }

    private static string GetStringValueOrThrow(TIObject tiObject, string attributeName) =>
        tiObject.GetAttributeValueAsString(attributeName) ?? throw new Exception(
            $"We expect the '{nameof(TIObject)}' to have a '{attributeName}', but it did not");
    
    private static Optional<string?> GetStringValue(TIObject tiObject, string attribute) =>
        tiObject.Attributes.Any(a => a.Name == attribute)
            ? new Optional<string?>(tiObject.GetAttributeValueAsString(attribute))
            : new Optional<string?>();

    private static Optional<long?> GetLongValue(TIObject tiObject, string attribute)
    {
        if (!tiObject.Attributes.Any(a => a.Name == attribute))
        {
            return new Optional<long?>();
        }

        var stringValue = tiObject.GetAttributeValueAsString(attribute)?.Trim();
        
        if (IsNullOrEmpty(stringValue))
        {
            return new Optional<long?>(null);
        }

        return long.TryParse(stringValue, out var result) 
            ? new Optional<long?>(result) 
            : new Optional<long?>(null);
    }

    private static Optional<bool?> GetBoolValue(TIObject tiObject, string attribute)
    {
        if (!tiObject.Attributes.Any(a => a.Name == attribute))
        {
            return new Optional<bool?>();
        }

        var stringValue = tiObject.GetAttributeValueAsString(attribute)?.Trim();
        
        if (IsNullOrEmpty(stringValue))
        {
            return new Optional<bool?>(null);
        }

        return stringValue.Equals("Y", StringComparison.OrdinalIgnoreCase) 
            ? new Optional<bool?>(true) 
            : new Optional<bool?>(false);
    }

    private static OptionalWithNull<int?> GetIntValueWithNull(TIObject tiObject, string attribute)
    {
        if (!tiObject.Attributes.Any(a => a.Name == attribute))
        {
            return new OptionalWithNull<int?>();
        }

        var stringValue = tiObject.GetAttributeValueAsString(attribute);
        return OptionalWithNull<int?>.ParseInt(stringValue);
    }

    private static OptionalWithNull<string?> GetStringValueWithNull(TIObject tiObject, string attribute)
    {
        if (!tiObject.Attributes.Any(a => a.Name == attribute))
        {
            return new OptionalWithNull<string?>();
        }

        var stringValue = tiObject.GetAttributeValueAsString(attribute);
        return OptionalWithNull<string?>.ParseString(stringValue);
    }

    private static OptionalWithNull<DateTime?> GetDateValueWithNull(TIObject tiObject, string attribute)
    {
        if (!tiObject.Attributes.Any(a => a.Name == attribute))
        {
            return new OptionalWithNull<DateTime?>();
        }

        var stringValue = tiObject.GetAttributeValueAsString(attribute);
        return OptionalWithNull<DateTime?>.ParseDateTime(stringValue);
    }

    private static Optional<DateTime?> GetDateValue(TIObject tiObject, string attribute)
    {
        if (tiObject.Attributes.All(a => a.Name != attribute))
        {
            return new Optional<DateTime?>();
        }

        var dateString = tiObject.GetAttributeValueAsString(attribute);
        
        if (IsNullOrWhiteSpace(dateString))
        {
            return new Optional<DateTime?>();
        }

        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateTime))
        {
            return new Optional<DateTime?>(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
        }

        return new Optional<DateTime?>();
    }

    private static Category? GetCategoryFromStatus(TIObject tiObject)
    {
        var status = tiObject.GetAttributeValueAsString(Status)?.Trim();
        if (IsNullOrEmpty(status))
        {
            return null;
        }

        if (!string.Equals(status, Category.PB.ToString(), StringComparison.OrdinalIgnoreCase) 
            && !string.Equals(status, Category.PA.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return string.Equals(status, Category.PA.ToString(), StringComparison.OrdinalIgnoreCase)
            ? Category.PA
            : Category.PB;
    }
}
