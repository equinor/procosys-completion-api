namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
/// <summary>
/// ImportOptions corresponds with the previous ProCoSys' "YallaYalla" flags
/// They all should be named and understood to default normal operation as "false", and isn't Nullable
/// </summary>
public class ImportOptions
{
    /// <summary>
    /// True: To be able to update ex. tags regardless of the existing TransferCode
    /// </summary>
    public bool ImportOptionIgnoreTransferCode { get; set; }
    /// <summary>
    /// True: Will no write data to the database, but will still run all validations and rules. For ex a Contractor to test his imports without making the database dirty.
    /// </summary>
    public bool ImportOptionNoWrite { get; set; }
    /// <summary>
    /// True: Handle Tag-Document as add/delete as a delta set. False: Handle as TheseOnly. Message metadata equivalent TAG_DOCUMENT_HANDLING_DELTA.
    /// </summary>
    public bool ImportOptionTagDocumentRelationHandleAsDelta { get; set; }

    /// <summary>
    /// True: Handle Tag-Document in the old way.
    /// </summary>
    public bool ImportOptionTagDocumentRelationOldHandler { get; set; }

    public ImportOptions()
    {
        ImportOptionIgnoreTransferCode = false;
        ImportOptionNoWrite = false;
        ImportOptionTagDocumentRelationHandleAsDelta = false; //Default: Handle these relations as "TheseOnly"
        ImportOptionTagDocumentRelationOldHandler = false;
    }

    public static ImportOptions FromSettings(IList<string> settings)
        => new ImportOptions
        {
            ImportOptionIgnoreTransferCode = settings.Contains("IMPORTOPTIONIGNORETRANSFERCODE"),
            ImportOptionNoWrite = settings.Contains("IMPORTOPTIONNOWRITE"),
            ImportOptionTagDocumentRelationOldHandler =
                settings.Contains("IMPORTOPTIONTAGDOCUMENTRELATIONOLDHANDLER")
        };
}
