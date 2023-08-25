using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;
/// <summary>
/// Contains a set of conversion utilities from TIE Message content to PCSInXXXX classes
/// </summary>
public static class TIEPCSCommonConverters
{
    /// <summary>
    /// Fills in method on the PcsObjectIn from the method/action on TIE object/message
    /// </summary>
    /// <param name="tieObject"></param>
    /// <param name="message"></param>
    /// <param name="pcsObjectToBeFilledIn"></param>
    public static void FillInCommandVerbToPerformFromTieObject(TIObject tieObject, TIInterfaceMessage message, IPcsObjectIn pcsObjectToBeFilledIn)
    {

        switch (GetCommandVerbToPerformFromTieObject(tieObject, message))
        {
            case "UPDATE":
                pcsObjectToBeFilledIn.ImportMethod = ImportMethod.Update;
                break;
            case "CREATE":
                pcsObjectToBeFilledIn.ImportMethod = ImportMethod.Create;
                break;
            case "MODIFY":
                pcsObjectToBeFilledIn.ImportMethod = ImportMethod.Modify;
                break;
            case "DELETE":
                pcsObjectToBeFilledIn.ImportMethod = ImportMethod.Delete;
                break;
        }
    }

    /// <summary>
    /// Gets from Tie object (try 1) or from the parent message (try 2), the command verb to perform.
    /// Will return CREATE, UPDATE, MODIFY or DELETE
    /// </summary>
    /// <param name="tieObject"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string GetCommandVerbToPerformFromTieObject(TIObject tieObject, TIInterfaceMessage message)
    {
        //Read the command verb from the TIE object/message, or assign the default.
        string lstrCommandVerb = null;

        //Priority: From object -> From message -> Default
        lstrCommandVerb = tieObject.Method;
        if (string.IsNullOrWhiteSpace(lstrCommandVerb)) lstrCommandVerb = message.Action;

        //No verb from inputs: MODIFY is default command to perform, e.g. create the object if it doesn't exist, or update it if it exists
        if (string.IsNullOrWhiteSpace(lstrCommandVerb)) lstrCommandVerb = "MODIFY";

        //Force to uppercase
        lstrCommandVerb = lstrCommandVerb.ToUpper();

        //Translate from TIE verbs to PCS verbs if they deviate, and evt force to default if verb isn't valid
        switch (lstrCommandVerb)
        {
            case "ALLOCATE":
                //2013-Mar-07: Is a new method introduced with the Doc/Tag allocation process (TADAA).
                //For ProCoSys it simply means "create"
                lstrCommandVerb = "CREATE";
                break;
            case "APPEND":
                lstrCommandVerb = "MODIFY";
                break;
            case "UPDATE":
                //OK
                break;
            case "CREATE":
                //OK
                break;
            case "INSERT":
                lstrCommandVerb = "CREATE";
                break;
            case "DELETE":
                //OK
                break;
            default:
                lstrCommandVerb = "MODIFY";
                break;
        }
        return lstrCommandVerb;
    }

    public static void UpdatePcsObjectImportOptionsFromTieMetadataAndConfiguration(IPcsObjectIn pcsObject, TIInterfaceMessage message, ImportOptions importOptionsDefault, ImportOptions importOptionsIgnore)
    {
        //GENERAL IMPORT WORKER for any Pcs in-class. Assigns values to the PCS object using Metadata input from the TIE message
        //If new import options get defined, this is the function to update. (...and the various place in the framework that need to obey them...)

        if (pcsObject == null) return;

        //Run in the defaults
        if (importOptionsDefault.ImportOptionIgnoreTransferCode &&
            !importOptionsIgnore.ImportOptionIgnoreTransferCode)
        {
            pcsObject.ImportOptions.ImportOptionIgnoreTransferCode = true;
        }
        if (importOptionsDefault.ImportOptionNoWrite &&
            !importOptionsIgnore.ImportOptionNoWrite)
        {
            pcsObject.ImportOptions.ImportOptionNoWrite = true;
        }
        if (importOptionsDefault.ImportOptionTagDocumentRelationOldHandler &&
            !importOptionsIgnore.ImportOptionTagDocumentRelationOldHandler)
        {
            pcsObject.ImportOptions.ImportOptionTagDocumentRelationOldHandler = true;
        }

        //Read from Metadata: Get from the message (only the presence of a certain one is enough to set it, regardless of its value
        if (message?.Metadata != null)
        {
            foreach (var bMetadata in message.Metadata)
            {
                switch (bMetadata.Name.ToUpper())
                {
                    case "IMPORTOPTIONIGNORETRANSFERCODE":
                        if (!importOptionsIgnore.ImportOptionIgnoreTransferCode)
                        {
                            pcsObject.ImportOptions.ImportOptionIgnoreTransferCode = true;
                        }
                        break;
                    case "IMPORTOPTIONNOWRITE":
                        if (!importOptionsIgnore.ImportOptionNoWrite)
                        {
                            pcsObject.ImportOptions.ImportOptionNoWrite = true;
                        }
                        break;
                    case "TAG_DOCUMENT_HANDLING_DELTA":
                        if (bMetadata.Value.GetValueAsBool())
                        {
                            pcsObject.ImportOptions.ImportOptionTagDocumentRelationHandleAsDelta = true;
                        }
                        break;
                    case "IMPORTOPTIONTAGDOCUMENTRELATIONOLDHANDLER":
                        if (!importOptionsIgnore.ImportOptionTagDocumentRelationOldHandler)
                        {
                            pcsObject.ImportOptions.ImportOptionTagDocumentRelationOldHandler = true;
                        }
                        break;
                }
            }
        }
    }
}
