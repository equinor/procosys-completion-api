using System.Reflection;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;
/// <summary>
/// Contains a set of conversion utilities from TIE Message content to PCSInXXXX classes
/// </summary>
public static class TIEPCSCommonConverters
{
    /// <summary>
    /// Checks that the TIE object has the bare common minimum to be interpreted/converted further on
    /// </summary>
    public static ImportResult? ValidateTieObjectCommonMinimumRequirements(TIObject tieObject, ILogger logger)
    {
        var attMissing = new List<string>();
        if (string.IsNullOrEmpty(tieObject.ObjectClass))
        {
            attMissing.Add("Class");
        }

        if (string.IsNullOrEmpty(tieObject.Site))
        {
            attMissing.Add("Site");
        }

        if (attMissing.Count > 0)
        {
            var message = $"Missing required attribute(s): {string.Join(",", attMissing)}";
            logger.LogError(message);
            return ImportResult.SingleError(message);
        }

        return null;
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

    public static void UpdatePcsObjectFromTieProperties(IPcsObject pcsObject, TIBaseObject tieObject)
    {
        //The TIEMapper does not handle additional mapping of subobjects and objects in relations, too much to refactor there to fix, so do it here for the moment.
        if (pcsObject == null || tieObject == null) return;
        PropertyInfo pcsProperty;
        var pcsObjectType = pcsObject.GetType();

        if (tieObject.Classification != null)
        {
            //Maps to ProCoSys Register, check whether there
            pcsProperty = pcsObjectType.GetProperty("Register", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (pcsProperty != null)
            {
                if (pcsProperty.GetValue(pcsObject) == null) pcsProperty.SetValue(pcsObject, tieObject.Classification, null);
            }
        }

        if (tieObject.Project != null)
        {
            //Maps to ProCoSys Project, check whether there
            pcsProperty = pcsObjectType.GetProperty("Project", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (pcsProperty != null)
            {
                if (pcsProperty.GetValue(pcsObject) == null) pcsProperty.SetValue(pcsObject, tieObject.Project, null);
            }
        }
    }

    /// <summary>
    /// Method for going through all attributes of a tieObject,
    /// and updating corresponding values at the pcsObject.
    /// </summary>
    /// <param name="pcsObject">object to be updated</param>
    /// <param name="tieAttributes">TI Attributes of incoming TIE object</param>
    /// <param name="disabledFieldLookup">lookup for disabled fields</param>
    /// <returns>Potential additional fields from the Tie attributes not found as Property nor Field of the ImpExp object</returns>
    public static List<AdditionalFieldForImport> UpdatePcsObjectFromTieAttributes(IPcsObject pcsObject, IEnumerable<TIAttribute> tieAttributes)
    {
        //GENERAL IMPORT WORKER for any Pcs  class. Assigns values to the PCS object using input from the TIE object
        //Run reflection match towards the incoming attribute names (case insensitive) versus fields/properties
        var llistAdditionalFieldsReturned = new List<AdditionalFieldForImport>();
        var pcsObjectType = pcsObject.GetType();

        foreach (var currTieAtt in tieAttributes)
        {
            //2013-Apr-03:[KRS]Sometimes the mapper (or other) makes a Name entry that is blank, this ruins eventual alias. So skip it
            if (currTieAtt.Name.ToUpper() == "NAME" && string.IsNullOrWhiteSpace(currTieAtt.Value)) continue;
            //2012-Aug-21:[KRS]Put properties up front. The objects are coded most likely with properties, so we run through those first, before trying on fields
            var pcsProperty = pcsObjectType.GetProperty(currTieAtt.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ??
                                       pcsObjectType.GetProperty(currTieAtt.Name + "id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (pcsProperty != null)
            {
                //***NATIVE PROPERTY FOUND
                //2014-Feb-04:[KRS]Previously we honored an incoming null value on the TIE object and shipped that further on as a blank for blanking
                //According new TIE details, these should be skipped. A special blanking signal has been introduced: the string '{NULL}'
                //
                if (!string.IsNullOrWhiteSpace(currTieAtt.Value))
                {
                    try
                    {
                        if (currTieAtt.Value.Equals("{NULL}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //Set blanking signals
                            if (pcsProperty.PropertyType == typeof(string))
                            {
                                pcsProperty.SetValue(pcsObject, "", null); //Empty string is blanking signal for string
                            }
                            else if (pcsProperty.PropertyType == typeof(DateTime?) || pcsProperty.PropertyType == typeof(DateTime))
                            {
                                pcsProperty.SetValue(pcsObject, DateTime.MinValue, null); //Blanking signal for DateTime
                            }
                        }
                        else
                        {
                            //Normal path
                            if (pcsProperty.PropertyType == typeof(bool?) ||
                                pcsProperty.PropertyType == typeof(bool))
                            {
                                //Special handling of boolean to be flexible on input
                                pcsProperty.SetValue(pcsObject, currTieAtt.GetValueAsBool(), null);
                            }
                            else if (pcsProperty.PropertyType == typeof(double?) ||
                                     pcsProperty.PropertyType == typeof(double))
                            {
                                //Special handling of floats to handle various decimal point characters
                                pcsProperty.SetValue(pcsObject,
                                                     (double)NumberConverter.ConvertToDecimal(currTieAtt.Value),
                                                     null);
                            }
                            else if (pcsProperty.PropertyType == typeof(float?) ||
                                     pcsProperty.PropertyType == typeof(float))
                            {
                                //Special handling of floats to handle various decimal point characters
                                pcsProperty.SetValue(pcsObject,
                                                     (float)NumberConverter.ConvertToDecimal(currTieAtt.Value),
                                                     null);
                            }
                            else if (pcsProperty.PropertyType == typeof(string))
                            {
                                //Strings, sure that these are trimmed
                                pcsProperty.SetValue(pcsObject,
                                                     currTieAtt.Value.Trim(),
                                                     null);
                            }
                            else
                                pcsProperty.SetValue(pcsObject, currTieAtt.GetValue(pcsProperty.PropertyType), null);
                            //Any other type goes here
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(
                            $"Failed to cast to '{pcsProperty.PropertyType}' given value '{currTieAtt.Value}' in field {currTieAtt.Name}", e);
                    }
                }
                continue; //Found as native property, so go to next attribute on Tie Object
            }

            //***ADDITIONAL FIELD assumed
            //Nope, not found, we must assume that this is a ProCoSys additional field. (As agreed with Trym per August 2012)
            //Trym mht inside ProCoSys for NUMBER and DATE datatypes:
            //Date valideres slik:
            //Convert.ToDateTime(value, CultureInfo.InvariantCulture);
            //Number valideres slik:
            //Convert.ToDecimal(value, CultureInfo.InvariantCulture.NumberFormat);

            //2014-Feb-04:[KRS]Previously we honored an incoming null value on the TIE object and shipped that further on as a blank for blanking
            //According new TIE details, these should be skipped. A special blanking signal has been introduced: the string '{NULL}'
            if (string.IsNullOrWhiteSpace(currTieAtt.Value) || string.IsNullOrWhiteSpace(currTieAtt.Name))
            {
                continue; //skip it
            }
            var addnField = currTieAtt.Value.Equals("{NULL}", StringComparison.InvariantCultureIgnoreCase)
                ? new AdditionalFieldForImport { ColumnName = currTieAtt.Name, Value = "" }
                : new AdditionalFieldForImport { ColumnName = currTieAtt.Name, Value = currTieAtt.Value.Trim() };

            fnAddToListOfAdditionalFieldsUniquely(llistAdditionalFieldsReturned, addnField);
        }
        return llistAdditionalFieldsReturned;
    }

    private static void fnAddToListOfAdditionalFieldsUniquely(List<AdditionalFieldForImport> listOfAddnFields, AdditionalFieldForImport addnField)
    {
        if (addnField == null) return;
        if (listOfAddnFields == null)
        {
            listOfAddnFields = new List<AdditionalFieldForImport>();
            listOfAddnFields.Add(addnField);
        }
        else
        {
            //make sure not there from before of same name
            if (!string.IsNullOrWhiteSpace(addnField.ColumnName))
            {
                if (!listOfAddnFields.Exists(a =>
                    a.ColumnName.Equals(addnField.ColumnName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    listOfAddnFields.Add(addnField);
                }
            }
        }
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
