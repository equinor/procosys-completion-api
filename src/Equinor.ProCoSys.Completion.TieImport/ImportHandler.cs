using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
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
                ImportObject(message, tiObject);
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

        //TODO: 109642 ImportResultHasError

        //TODO: 109739 _messageInspector.UpdateImportOptions(proCoSysImportObject, message);

        //TODO: 106692 CustomImport

        //TODO: 106693 NCR special handling

        //TODO: 107052 Create command and send to command handler

        //TODO: 106687 CommandFailureHandler;

        //TODO: 109642 return ImportResult.Ok();
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
}
