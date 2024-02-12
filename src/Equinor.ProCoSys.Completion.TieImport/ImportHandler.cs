using Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Authentication;

namespace Equinor.ProCoSys.Completion.TieImport;

public class ImportHandler : IImportHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IImportSchemaMapper _importSchemaMapper;
    private readonly ILogger<ImportHandler> _logger;
    //private readonly IMediator _mediator;

    public ImportHandler(IServiceScopeFactory serviceScopeFactory, IImportSchemaMapper importSchemaMapper, ILogger<ImportHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _importSchemaMapper = importSchemaMapper;
        _logger = logger;
        //_mediator = mediator;
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

            tiMessageResult = ImportMessage(mapped.Message!);
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
        _logger.LogInformation("To import message GUID={MessageGuid} with {MessageCount} object(s)", message.Guid, message.Objects.Count);
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
            _logger.LogError("Failed to import message with GUID={MessageGuid} Exception: {ExceptionMessage}, InnerException {InnerExceptionMessage}", 
                message.Guid, ex.Message, ex.InnerException?.Message);
        }
        finally
        {
            //This is where existing code does commit or abort...
        }

        //TODO: 109642 return tiMessageResult;
        return new TIMessageResult();
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

        //TODO: XXXXXX Need validation here


        using var scope = _serviceScopeFactory.CreateScope();
        
        var plantSetter = scope.ServiceProvider.GetRequiredService<IPlantSetter>();
        plantSetter.SetPlant(message.Site);

        var currentUserSetter = scope.ServiceProvider.GetRequiredService<ICurrentUserSetter>();
        //currentUserSetter.SetCurrentUserOid(new Guid("53731422-9D09-4871-BC8A-9E487B3CF89D"));

        var ipoDevClientId = new Guid("2e1868db-3024-45a9-b3f1-568e85586244");
        currentUserSetter.SetCurrentUserOid(ipoDevClientId);

        var authenticatorOptions = scope.ServiceProvider.GetRequiredService<IAuthenticatorOptions>();

        var mainApiAuthenticator = scope.ServiceProvider.GetRequiredService<IMainApiAuthenticator>();
        mainApiAuthenticator.AuthenticationType = AuthenticationType.AsApplication;

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        //TODO: JSOI move to constant
        var description = tiObject.GetAttributeValueAsString("Description");
        if (description is null)
        {
            throw new ArgumentException("Temp error to be replaced by validation: description is null");
        }

        var checkListGuid = new Guid("EB38CCBC-C659-D926-E053-2810000AC5B2");
        var projectGuid = new Guid("EB38367C-37DE-DD39-E053-2810000A174A");
        var raisedByOrgGuid = new Guid("46A76B8B-F7BC-4BAB-9C19-81A64A550250");
        var clearingByOrgGuid = new Guid("72EA41A7-6283-4ED4-B910-B4FC38B391DD");

        var createPunchCommand = new CreatePunchItemCommand(Category.PB, description, projectGuid, checkListGuid,
            raisedByOrgGuid, clearingByOrgGuid, null, null, null, null, 
            null, null, null, null, null, null, 
            null, false, null, null);
        
        //TODO: JSOI Make method async
        var result = mediator.Send(createPunchCommand).GetAwaiter().GetResult();

        //TODO: 106687 CommandFailureHandler;

        //TODO: 109642 return ImportResult.Ok();
    }


    private TIMessageResult HandleExceptionFromImportOperation(TIInterfaceMessage message, Exception e)
    {
        var tiMessageResult = e.ToMessageResult();
        _logger.LogError(
            "Error when committing message. Exception: {ExceptionMessage} Stacktrace: {StackTrace} TIEMessage: {TieMessage}",
            e.Message, e.StackTrace, message);

        return tiMessageResult;
    }

    private static void AddResultOfImportOperationToResponseObject(TIInterfaceMessage message, TIMessageResult? tiMessageResult,
        TIResponseFrame response)
    {
        if (tiMessageResult is not null)
        {
            // Observe: The ExternalReference is copied over to the result; this is where we keep/pass back the ReceiptID.
            tiMessageResult.Guid = message.Guid;
            tiMessageResult.ExternalReference = message.ExternalReference;
            response.Results.Add(tiMessageResult);
        }
    }
}
