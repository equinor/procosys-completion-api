using System.Reflection;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Converters;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
using MediatR;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport;

public class ImportHandler : IImportHandler
{
    private readonly IImportSchemaMapper _importSchemaMapper;
    private readonly ILogger<ImportHandler> _logger;

    public ImportHandler(IImportSchemaMapper importSchemaMapper, ILogger<ImportHandler> logger)
    {
        _importSchemaMapper = importSchemaMapper;
        _logger = logger;
    }
    public TIResponseFrame Handle(TIInterfaceMessage? message)
    {
        var response = new TIResponseFrame();
        if (message is null)
        {
            _logger.LogWarning("Received an empty message. Skipped.");
            return response;
        }

        _logger.LogInformation("To import a message with name {ObjectName}, Class {ObjectClass}, Site {Site}.", 
            message.ObjectName, message.ObjectClass, message.Site);


        TIMessageResult? tiMessageResult = null;
        try
        {
            //TODO: 106683 ObjectFixers

            var mapped = _importSchemaMapper.Map(message);

            if (mapped.Success is false)
            {
                tiMessageResult = mapped.ErrorResult;
                return response;
            }

            //TODO: 106685 PostMapperFixer;

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
        //TODO: 109642 Collect errors and warnings
        try
        {
            foreach (var tiObject in message.Objects)
            {
                LogImportStarting(tiObject);

                ImportObject(message, tiObject);

                LogImportIsDone(tiObject);
            }
        }
        catch (Exception ex) //TODO: 109642 SetFailed result
        {
            _logger.LogError($"Exception: {ex.Message}, InnerException {ex.InnerException?.Message}");
        }
        finally
        {
            //This is where existing code does commit or abort...
        }

        //TODO: 109642 return tiMessageResult;
        return new TIMessageResult(); //TODO: Dummy for now
    }

    private void ImportObject(TIInterfaceMessage message, TIObject tiObject)
    {
        //TODO: 105834 CollectWarnings

        //TODO: 106686 MapRelationsUntilTieMapperGetsFixed

        //TODO: 106699 TIEProCoSysMapperCustomMapper.CustomMap

        //TODO: 109739 _messageInspector.CheckForScriptInjection(tiObject);
        //TODO: 109738 TIEPCSCommonConverters.ValidateTieObjectCommonMinimumRequirements(tiObject, _logger);

        //TODO: 109642 ImportResultHasError

        //TODO: 106691 SiteSpecificHandler

        var proCoSysImportObject = CreateProCoSysImportObject(tiObject);

        //TODO: 109642 ImportResultHasError

        //TODO: 109739 _messageInspector.UpdateImportOptions(proCoSysImportObject, message);

        EnsureObjectNameHasValue(tiObject, proCoSysImportObject);

        //TODO: 106692 CustomImport

        var incomingObjectType = proCoSysImportObject.GetType();

        //TODO: 106693 NCR special handling

        var command = CreateCommand(incomingObjectType, proCoSysImportObject);

        //TODO: 106687 CommandFailureHandler;

        //TODO: 109642 return ImportResult.Ok();
    }

    private static void EnsureObjectNameHasValue(TIObject tiObject, IPcsObjectIn pcsObject)
    {
        if (string.IsNullOrWhiteSpace(tiObject.ObjectName))
        {
            tiObject.ObjectName = ((PcsaObjectIn) pcsObject).Name;
        }
    }
    private IIsProjectCommand CreateCommand(Type incomingObjectType, IPcsObjectIn pcsObjectIn)
    {
        //Always return a CreatePunchItemCommand for now
        var punchItemIn = pcsObjectIn as PcsPunchItemIn;
        if (punchItemIn is null)
        {
            throw new InvalidCastException($"Not able to cast {incomingObjectType} to {nameof(PcsPunchItemIn)}");
        }

        var createPunchCommand = new CreatePunchItemCommand(Category.PA, punchItemIn.Description, new Guid(), new Guid(), new Guid(),
            new Guid(), new Guid(), null, null, null, null, null, null, null, null, null, null, false, null, null);
        return createPunchCommand;
    }

    private IPcsObjectIn? CreateProCoSysImportObject(TIObject tieObject)
    {
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

        //TODO: 109642 ImportResult.SingleError(message);
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
        if (tieObject.Site is null)
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

        //TODO: 106693 NCR special handling
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
