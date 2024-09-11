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
            tiObject.Project,
            tiObject.Site,
            GetStringValueOrThrow(tiObject, TagNo),
            GetStringValueOrThrow(tiObject, ExternalPunchItemNo),
            GetStringValueOrThrow(tiObject, FormType),
            GetStringValueOrThrow(tiObject, Responsible),
            GetStringValue(tiObject, PunchItemNo),
            GetStringValue(tiObject, Description),
            GetStringValue(tiObject, RaisedByOrganization),
            GetCategoryFromStatus(tiObject),
            GetStringValue(tiObject, PunchListType),
            GetDateValue(tiObject, DueDate),
            GetDateValue(tiObject, ClearedDate),
            GetStringValue(tiObject, ClearedBy),
            GetStringValue(tiObject, ClearedByOrganization),
            GetDateValue(tiObject, VerifiedDate),
            GetStringValue(tiObject, VerifiedBy),
            GetDateValue(tiObject, RejectedDate),
            GetStringValue(tiObject, RejectedBy),
            GetStringValue(tiObject, MaterialRequired),
            GetDateValue(tiObject, MaterialEta),
            GetStringValue(tiObject, MaterialNo)
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
    
    private static Optional<DateTime?> GetDateValue(TIObject tiObject, string attribute)
    {
        var dueDate = tiObject.GetAttributeValueAsString(attribute);
        if (tiObject.Attributes.All(a => a.Name != attribute))
        {
            return new Optional<DateTime?>();
        }

        if (IsNullOrEmpty(dueDate))
        {
            return new Optional<DateTime?>();
        }

        var dateTime = DateTime.SpecifyKind(DateTime.Parse(dueDate), DateTimeKind.Utc);

        return new Optional<DateTime?>(dateTime);
    }

    private static Category? GetCategoryFromStatus(TIObject tiObject)
    {
        var status = tiObject.GetAttributeValueAsString(Status);
        if (IsNullOrEmpty(status))
        {
            return null;
        }

        if (!string.Equals(status,Category.PB.ToString(), StringComparison.InvariantCultureIgnoreCase) 
            && !string.Equals(status ,Category.PA.ToString(), StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return status == Category.PA.ToString()
            ? Category.PA
            : Category.PB;
    }
}
