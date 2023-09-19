using System.Reflection;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
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


        TIMessageResult? tiMessageResult = null;
        try
        {
            //TODO: Call ObjectFixers.Fix
            
            var mapped = _importSchemaMapper.Map(message);

            if (mapped.Success is false)
            {
                tiMessageResult = mapped.ErrorResult;
                return response;
            }

            //TODO: _postMapperFixer.Fix(message);

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

        var tiMessageResult = new TIMessageResult { Result = MessageResults.Successful };

        try
        {
            foreach (var tiObject in message.Objects)
            {
                LogImportStarting(tiObject);

                var importResult = ImportObject(message, tiObject);

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

    private ImportResult? ImportObject(TIInterfaceMessage message, TIObject tiObject)
    {
        //TODO: Do 105834 CollectWarnings here

        TIEProCoSysMapperCustomMapper.MapRelationsUntilTieMapperGetsFixed(tiObject);
        TIEProCoSysMapperCustomMapper.CustomMap(tiObject, message);

        _messageInspector.CheckForScriptInjection(tiObject);
        var importResult = TIEPCSCommonConverters.ValidateTieObjectCommonMinimumRequirements(tiObject, _logger); //TODO: Remove _logger parameter
        if (ImportResultHasError(importResult))
        {
            return importResult;
        }

        //TODO: Call SiteSpecificHandler

        var proCoSysImportObject = CreateProCoSysImportObject(tiObject, out importResult);

        if (ImportResultHasError(importResult))
        {
            return importResult;
        }

        TIEPCSCommonConverters.FillInCommandVerbToPerformFromTieObject(
            tiObject,
            message,
            proCoSysImportObject);

        _messageInspector.UpdateImportOptions(proCoSysImportObject, message);

        EnsureObjectNameHasValue(tiObject, proCoSysImportObject);

        //TODO: TIEProCoSysImportCustomImport.CustomImport

        var incomingObjectType = proCoSysImportObject.GetType();

        //TODO: NCR special handling

        var command = CreateCommand(incomingObjectType, proCoSysImportObject.ImportMethod, proCoSysImportObject);

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

    }

    private static void EnsureObjectNameHasValue(TIObject tiObject, IPcsObjectIn pcsObject)
    {
        if (string.IsNullOrWhiteSpace(tiObject.ObjectName))
        {
            tiObject.ObjectName = ((PcsaObjectIn) pcsObject).Name;
        }
    }

    private static bool ImportResultHasError(ImportResult? importResult) => importResult != null;

    private IIsProjectCommand CreateCommand(Type incomingObjectType, ImportMethod pcsObjectImportMethod, IPcsObjectIn pcsObjectIn)
    {
        //Always return a CreatePunchItemCommand for now
        var punchItemIn = pcsObjectIn as PcsPunchItemIn;
        if (punchItemIn is null)
        {
            throw new InvalidCastException($"Not able to cast {incomingObjectType} to {nameof(PcsPunchItemIn)}");
        }


        //TODO: JSOI Run validation before retrieving guids??
        //TODO: JSOI Retrieve guids from strings
        //Observation: PCS5 uses "strongly typed" commands while PCS4 uses "primitive typed" commands (i.e. string, int, datetime
        //var createPunchCommand = new CreatePunchItemCommand(punchItemIn.Description, punchItemIn.RaisedByOrganization, punchItemIn.ClearedByOrganization, punchItemIn.Priority, punchItemIn.sortingGuid, punchItemIn.PunchListType);
        var createPunchCommand = new CreatePunchItemCommand(punchItemIn.Description, new Guid(), new Guid(), new Guid());
        return createPunchCommand;
    }


    private IPcsObjectIn CreateProCoSysImportObject(TIObject tieObject, out ImportResult? result)
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
                return TIE2PCSPunchItemConverter.PopulateProCoSysPunchItemImportObject(tieObject);
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

    private void LogImportStarting(TIObject tiObject) => _logger.LogInformation(tiObject.GetLogFriendlyString() + ": Starting import of object.");

    private void LogImportIsDone(TIObject tiObject) => _logger.LogInformation(tiObject.GetLogFriendlyString() + ": Import done.");

    private void LogImportResultUnsuccessful(TIObject tiObject)
    {
        _logger.LogInformation(tiObject.GetLogFriendlyString() + ": Import of object unsuccessful.");
        _logger.LogInformation("Error is causing rest of message to be dropped.");
    }
}
