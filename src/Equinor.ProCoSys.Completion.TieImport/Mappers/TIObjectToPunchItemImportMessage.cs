using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;

public static class TiObjectToPunchItemImportMessage
{
    public static PunchItemImportMessage ToPunchItemImportMessage(TIObject tiObject)
    {
        var foo = new PunchItemImportMessage(
            tiObject.Guid,
            tiObject.Site,
            GetStringValue(tiObject, "CLASS"),
            GetStringValue(tiObject, "EXTERNALPUNCHITEMNO"),
            GetStringValue(tiObject, "TAGNO"),
            GetStringValue(tiObject, "PUNCHITEMNO"),
            GetStringValue(tiObject, "DESCRIPTION"),
            GetStringValue(tiObject, "RESPONSIBLE"),
            GetStringValue(tiObject, "RAISEDBYORGANIZATION"),
            GetStringValue(tiObject, "STATUS"),
            GetStringValue(tiObject, "FORMTYPE"),
            GetStringValue(tiObject, "PUNCHLISTTYPE"),
            GetDateValue(tiObject, "DUEDATE"),
            GetDateValue(tiObject, "CLEAREDDATE"),
            GetStringValue(tiObject, "CLEAREDBY"),
            GetStringValue(tiObject, "CLEAREDBYORGANIZATION"),
            GetDateValue(tiObject, "VERIFIEDDATE"),
            GetStringValue(tiObject, "VERIFIEDBY"),
            GetDateValue(tiObject, "REJECTEDDATE"),
            GetStringValue(tiObject, "REJECTEDBY"),
            GetStringValue(tiObject, "MATERIALREQUIRED"),
            GetDateValue(tiObject, "MATERIALETA"),
            GetStringValue(tiObject, "MATERIALNO")
        );

        return foo;
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
