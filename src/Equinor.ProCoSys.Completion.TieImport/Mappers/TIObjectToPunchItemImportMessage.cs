using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public static class TiObjectToPunchItemImportMessage
{
    public const string ProjectMissing = "$PROJECT_MISSING$";

    public static PunchItemImportMessage ToPunchItemImportMessage(TIObject tiObject)
    {
        var message = new PunchItemImportMessage(
            tiObject.Guid,
            tiObject.Site,
            tiObject.GetAttributeValueAsString(PunchObjectAttributes.Project) ?? throw new Exception($"We expect the '{nameof(TIObject)}' to have a '{PunchObjectAttributes.Project}', but it did not"),
            tiObject.GetAttributeValueAsString(PunchObjectAttributes.TagNo) ?? throw new Exception($"We expect the '{nameof(TIObject)}' to have a '{PunchObjectAttributes.TagNo}', but it did not"),
            GetStringValue(tiObject, PunchObjectAttributes.Class),
            GetStringValue(tiObject, PunchObjectAttributes.ExternalPunchItemNo),
            GetStringValue(tiObject, PunchObjectAttributes.PunchItemNo),
            GetStringValue(tiObject, PunchObjectAttributes.Description),
            GetStringValue(tiObject, PunchObjectAttributes.Responsible),
            GetStringValue(tiObject, PunchObjectAttributes.RaisedByOrganization),
            GetStringValue(tiObject, PunchObjectAttributes.Status),
            GetStringValue(tiObject, PunchObjectAttributes.FormType),
            GetStringValue(tiObject, PunchObjectAttributes.PunchListType),
            GetDateValue(tiObject, PunchObjectAttributes.DueDate),
            GetDateValue(tiObject, PunchObjectAttributes.ClearedDate),
            GetStringValue(tiObject, PunchObjectAttributes.ClearedBy),
            GetStringValue(tiObject, PunchObjectAttributes.ClearedByOrganization),
            GetDateValue(tiObject, PunchObjectAttributes.VerifiedDate),
            GetStringValue(tiObject, PunchObjectAttributes.VerifiedBy),
            GetDateValue(tiObject, PunchObjectAttributes.RejectedDate),
            GetStringValue(tiObject, PunchObjectAttributes.RejectedBy),
            GetStringValue(tiObject, PunchObjectAttributes.MaterialRequired),
            GetDateValue(tiObject, PunchObjectAttributes.MaterialEta),
            GetStringValue(tiObject, PunchObjectAttributes.MaterialNo)
        );

        return message;
    }

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
