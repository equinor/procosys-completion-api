using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;
using static Equinor.ProCoSys.Completion.Domain.Imports.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public static class TiObjectToPunchItemImportMessage
{
    public static PunchItemImportMessage ToPunchItemImportMessage(TIObject tiObject)
    {
        var message = new PunchItemImportMessage(
            tiObject.Guid,
            tiObject.Site,
            GetStringValueOrThrow(tiObject, Project),
            GetStringValueOrThrow(tiObject, TagNo),
            GetStringValueOrThrow(tiObject, ExternalPunchItemNo),
            GetStringValueOrThrow(tiObject, FormType),
            GetStringValue(tiObject, Class),
            GetStringValue(tiObject, PunchItemNo),
            GetStringValue(tiObject, Description),
            GetStringValue(tiObject, Responsible),
            GetStringValue(tiObject, RaisedByOrganization),
            GetStringValue(tiObject, Status),
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

    public static IReadOnlyCollection<PunchItemImportMessage> ToPunchItemImportMessages(List<TIObject> tiObjects) =>
        tiObjects.Select(ToPunchItemImportMessage).ToArray();

    private static Optional<string?> GetStringValue(TIObject tiObject, string attribute) =>
        tiObject.Attributes.Any(a => a.Name == attribute)
            ? new Optional<string?>(tiObject.GetAttributeValueAsString(attribute))
            : new Optional<string?>();

    private static Optional<DateTime?> GetDateValue(TIObject tiObject, string attribute) =>
        tiObject.Attributes.Any(a => a.Name == attribute)
            ? new Optional<DateTime?>(DateTime.Parse(tiObject.GetAttributeValueAsString(attribute)))
            : new Optional<DateTime?>();
}
