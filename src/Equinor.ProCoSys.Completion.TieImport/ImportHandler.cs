using System.Reflection;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Converters;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport;

public class ImportHandler : IImportHandler
{
    private readonly IImportSchemaMapper _importSchemaMapper;
    private readonly ILogger<ImportHandler> _logger;
    private readonly IMessageInspector _messageInspector;

    public ImportHandler(IImportSchemaMapper importSchemaMapper, ILogger<ImportHandler> logger, IMessageInspector messageInspector)
    {
        _importSchemaMapper = importSchemaMapper;
        _logger = logger;
        _messageInspector = messageInspector;
    }
    public TIResponseFrame Handle(TIInterfaceMessage? message)
    {
        var response = new TIResponseFrame();
        if (message == null)
        {
            _logger.LogWarning("Received an empty message. Skipped.");
            return response;
        }

        _logger.LogInformation("To import a message with name {ObjectName}, Class {ObjectClass}, Site {Site}.", 
            message.ObjectName, message.ObjectClass, message.Site);


        //TODO: Don't forget this one...
        //_objectFixers.Fix(message);

        TIMessageResult? tiMessageResult = null;
        try
        {
            var mapped = _importSchemaMapper.Map(message);

            if (mapped.Success is false)
            {
                tiMessageResult = mapped.ErrorResult;
                return response;
            }

            //WrapInUnitOfWork replaced by ImportMessage....
            tiMessageResult = ImportMessage(message);
        }
        catch (Exception e)
        {
            tiMessageResult = HandleExceptionFromImportOperation(message, e);
        }
        finally
        {
            AddResultOfImportOperationToResponseObject(message, tiMessageResult, response);
        }
        

        return response;
    }

    private TIMessageResult ImportMessage(TIInterfaceMessage message)
    {
        _logger.LogInformation($"To import message GUID={message.Guid} with {message.Objects.Count} object(s)");

        //TODO: JSOI
        //_postMapperFixer.Fix(message);

        var tiMessageResult = new TIMessageResult { Result = MessageResults.Successful };
        try
        {
            foreach (var tiObject in message.Objects)
            {
                LogImportStarting(tiObject);

                var importResult = ImportObject(message, tiObject);

                //---------------------------------Code in old ImportHandler
                importResult.AppendTo(tiMessageResult, tiObject);

                if (tiMessageResult.Result != MessageResults.Successful)
                {
                    LogImportResultUnsuccessful(tiObject);
                    break;
                }

                LogImportIsDone(tiObject);
            }
        }
        catch (Exception ex) when (SetFailed(tiMessageResult))
        {
            _logger.LogError($"Exception: {ex.Message}, InnerException {ex.InnerException?.Message}");
        }
        finally
        {
            //This is where existing code does commit or abort...
        }

        return tiMessageResult;
    }

    private void LogImportStarting(TIObject tiObject) => _logger.LogInformation(tiObject.GetLogFriendlyString() + ": Starting import of object.");

    private void LogImportIsDone(TIObject tiObject) => _logger.LogInformation(tiObject.GetLogFriendlyString() + ": Import done.");

    private void LogImportResultUnsuccessful(TIObject tiObject)
    {
        _logger.LogInformation(tiObject.GetLogFriendlyString() + ": Import of object unsuccessful.");
        _logger.LogInformation("Error is causing rest of message to be dropped.");
    }

    private ImportResult? ImportObject(TIInterfaceMessage message, TIObject tiObject)
    {
        //TODO: CollectWarnings
        TIEProCoSysMapperCustomMapper.MapRelationsUntilTieMapperGetsFixed(tiObject);
        TIEProCoSysMapperCustomMapper.CustomMap(tiObject, message);

        //------------------------TIEProCoSysServer.LoadObject
        _messageInspector.CheckForScriptInjection(tiObject);
        var importResult = TIEPCSCommonConverters.ValidateTieObjectCommonMinimumRequirements(tiObject, _logger); //TODO: Remove _logger parameter
        if (ImportResultHasError(importResult))
        {
            return importResult;
        }

        //SiteSpecificHandler.HandleSiteSpecific(tiObject, null, message, null);

        var pcsObject = CreateIPcsObjectIn(tiObject, out importResult);
        if (ImportResultHasError(importResult)) //Meaning an error occurred
        {
            // Abort, no further processing of objects.
            //Confusing, but if import result has error, it is not null
            return importResult!;
        }

        //----------Start PreparePcsObject
        // Assign the method on the pcsObject.
        TIEPCSCommonConverters.FillInCommandVerbToPerformFromTieObject(
            tiObject,
            message,
            pcsObject);

        // Get and translate what to do (MODIFY, DELETE etc), this is generic for any type.
        // This comes from the object or message header.
        // Set its ImportOptions.
        _messageInspector.UpdateImportOptions(pcsObject, message);

        // Make sure that Name is set on input on object for further logging purposes.
        if (string.IsNullOrWhiteSpace(tiObject.ObjectName))
        {
            tiObject.ObjectName = ((PcsaObjectIn)pcsObject).Name;
        }

        // Do eventual custom preparations of the object before shipping it to ProCoSys.
        //TODO: JSOI
        //try
        //{
        //    TIEProCoSysImportCustomImport.CustomImport(pcsObject, tieObject, message);
        //}
        //catch (Exception ex)
        //{
        //    Logger.Error($"CustomImport failed: {ex.Message}", ex);
        //}
        //----------End PreparePcsObject

        //var commandResult = LoadObject(pcsObject);
        //----------------------------Below is LoadObject code...
        if (pcsObject == null)
        {
            throw new Exception("Object to import is NULL");
        }

        var incomingObjectType = pcsObject.GetType();

        //TODO: JSOI Not needed for punch
        //// NCR is special since properties are read from document part.
        //var sourceObject = incomingObjectType == typeof(PcsNcrDocumentIn)
        //    ? ((PcsNcrDocumentIn)pcsObject).DocumentPart
        //    : pcsObject;

        var command = CreateCommand(incomingObjectType, pcsObject.ImportMethod);

        //TODO: JSOI
        //CopyProperties(sourceObject, command);
        //Use below method instead of CopyProperties name
        //PopulateCommandWithValues(command, pcsObject);

        //TODO: JSOI Send command to CreatePunchItem via MediatR


        //TODO: JSOI Temp code
        //Retrieve result from MediatR command
        var dummyCommandResultSuccess = true; //Temp for now
        if (!dummyCommandResultSuccess)
        {
            return ImportResult.Error(); //JSOI TEMP
            //    importResult = _commandFailureHandler.HandleFailureResult(commandResult);
            //    return ImportResult;
        }
        return ImportResult.Ok();

        //---------------------------------End of LoadObject

        //JSOI: TODO: Figure out importresult, is it OK now?
    }

    private static bool ImportResultHasError(ImportResult? importResult) => importResult != null;

    private object CreateCommand(Type incomingObjectType, ImportMethod pcsObjectImportMethod)
    {
        //Always return a CreatePunchItemCommand for now
        throw new NotImplementedException();
    }

    private void PreparePcsObject(TIInterfaceMessage message, TIObject tieObject, IPcsObjectIn pcsObject)
    {

    }

    private IPcsObjectIn CreateIPcsObjectIn(TIObject tieObject, out ImportResult? result)
    {
        result = null;
        var pcsObject = PcsFromTie(tieObject);

        if (pcsObject != null)
        {
            SetPlantIdFromTieObject(tieObject, pcsObject);

            var tieClassificationInfo = "";
            if (!string.IsNullOrWhiteSpace(tieObject.Classification))
            {
                tieClassificationInfo = $"({tieObject.Classification.ToUpper()})";
            }

            var debugInfo =
                $"Made ready for import: Site: {tieObject.Site}, {tieObject.ObjectClass.ToUpper()} {tieClassificationInfo} {pcsObject.Name}";
            _logger.LogDebug(debugInfo);

            return pcsObject;
        }

        var message = $"Import of Class {tieObject.ObjectClass}";
        if (!string.IsNullOrWhiteSpace(tieObject.Classification))
        {
            message += $", Classification {tieObject.Classification}";
        }

        message += " is not supported.";
        _logger.LogError(message);
        result = ImportResult.SingleError(message);
        return null;
    }

    private IPcsObjectIn? PcsFromTie(TIObject tieObject)
    {
        switch (tieObject.ObjectClass.ToUpper())
        {
            case "PUNCHITEM":
                return TIE2PCSPunchItemConverter.AssignPunchItemObject(tieObject);
            default:
                return null;
        }
    }
    private void SetPlantIdFromTieObject(TIObject tieObject, IPcsObjectIn pcsObject)
    {
        if (tieObject.Site == null)
        {
            return;
        }

        var pcsObjectType = pcsObject.GetType();
        var pcsProperty = pcsObjectType.GetProperty(
            "PlantId",
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (pcsProperty != null)
        {
            pcsProperty.SetValue(pcsObject, tieObject.Site, null);
        }

        //if (pcsObject is PcsNcrDocumentIn pcsNcrDocumentIn)
        //{
        //    pcsNcrDocumentIn.NcrPart.PlantId = tieObject.Site;
        //    pcsNcrDocumentIn.DocumentPart.PlantId = tieObject.Site;
        //}
    }

    private static bool SetFailed(TIMessageResult result)
    {
        result.Result = MessageResults.Failed;
        return false;
    }

    private TIMessageResult? HandleExceptionFromImportOperation(TIInterfaceMessage message, Exception e)
    {
        var tiMessageResult = e.ToMessageResult();
        _logger.LogError(
            $"Error when committing message. Exception: {e.Message} Stacktrace: {e.StackTrace} TIEMessage: {message}",
            e.Message, e.StackTrace, message);

        //if (e is AbortImportException)
        //{
        //    // Import of message frame to be aborted.
        //    Logger.Error("Error is causing rest of message frame to be dropped.");
        //    break;
        //}
        return tiMessageResult;
    }

    private static void AddResultOfImportOperationToResponseObject(TIInterfaceMessage message, TIMessageResult? tiMessageResult,
        TIResponseFrame response)
    {
        if (tiMessageResult != null)
        {
            // Observe: The ExternalReference is copied over to the result; this is where we keep/pass back the ReceiptID.
            tiMessageResult.Guid = message.Guid;
            tiMessageResult.ExternalReference = message.ExternalReference;
            response.Results.Add(tiMessageResult);
        }
    }

    private TIMessageResult WrapInUnitOfWork(TIInterfaceMessage mappedMessage)
    {
        //Hardcode happiness for now
        return new TIMessageResult {Result = MessageResults.Successful};
    }
}
