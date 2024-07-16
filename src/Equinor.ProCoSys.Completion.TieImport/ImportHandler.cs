using System.Diagnostics;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Authorization;
using System.Security.Claims;
using Equinor.ProCoSys.Auth.Misc;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.Tags;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.TieImport.Configuration;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class ImportHandler : IImportHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IImportSchemaMapper _importSchemaMapper;
    private readonly ILogger<ImportHandler> _logger;

    public ImportHandler(IServiceScopeFactory serviceScopeFactory, IImportSchemaMapper importSchemaMapper, 
             ILogger<ImportHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _importSchemaMapper = importSchemaMapper;
        _logger = logger;
    }

    public async Task<TIResponseFrame> Handle(TIInterfaceMessage? message)
    {
        var response = new TIResponseFrame();
        if (message is null)
        {
            _logger.LogWarning("Received an empty message. Skipped.");
            return response;
        }

        _logger.LogInformation("To import a message with name {ObjectName}, Class {ObjectClass}, Site {Site}.", 
            message.ObjectName, message.ObjectClass, message.Site);

        var sw = new Stopwatch();
        sw.Start();
        
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

            tiMessageResult = await ImportMessage(mapped.Message!);
        }
        catch (Exception e)
        {
            tiMessageResult = HandleExceptionFromImportOperation(message, e);
        }
        finally
        {
            AddResultOfImportOperationToResponseObject(message, tiMessageResult, response);
        }
        
        sw.Stop();
        _logger.LogCritical("Import elapsed {Elapsed}", sw.Elapsed);
        
        return response;
    }

    private async Task<TIMessageResult> ImportMessage(TIInterfaceMessage message)
    {
        _logger.LogInformation("To import message GUID={MessageGuid} with {MessageCount} object(s)", message.Guid, message.Objects.Count);

        var importResults = message.Objects
            .Select(PunchTiObjectValidator.Validate)
            .ToList()
            .Select(TiObjectToPunchItemImportMessage.ToPunchItemImportMessage)
            .ToList();
        
        using var scope = _serviceScopeFactory.CreateScope();
        var importDataFetcher = scope.ServiceProvider.GetRequiredService<IImportDataFetcher>();
        
        var contextBuilder = new PlantScopedImportDataContextBuilder(importDataFetcher);
        var scopedContext = await contextBuilder
            .BuildAsync(importResults
                .Where(x => x.Message is not null)
                .Select(x => x.Message!).ToArray(), CancellationToken.None);

        var messagesByPlant = importResults.GroupBy(x => x.Message?.Plant);

        var tasks = messagesByPlant
            .SelectMany(plantMessage =>
        {
            if (plantMessage.Key is null)
            {
                return plantMessage
                    .Select(Task.FromResult);
            }
            
            var context = scopedContext[plantMessage.Key];
            var mapper = new PunchItemImportMessageToCreatePunchItem(context);

            var commands = mapper
                .Map(plantMessage.ToArray());

            return commands.Select(c => ImportObjectExceptionWrapper(c, context, CancellationToken.None));
        });

        var results = await Task.WhenAll(tasks);
        
        // //TODO: 109642 Collect errors and warnings
        // try
        // {
        //     var foo = TiObjectToPunchItemImportMessage.ToPunchItemImportMessages(message.Objects);
        //     foreach (var tiObject in message.Objects)
        //     {
        //         await ImportObject(message, tiObject);
        //     }
        // }
        // catch (Exception ex) //TODO: 109642 SetFailed result
        // {
        //     _logger.LogError("Failed to import message with GUID={MessageGuid} Exception: {ExceptionMessage}, InnerException {InnerExceptionMessage}", 
        //         message.Guid, ex.Message, ex.InnerException?.Message);
        // }
        // finally
        // {
        //     //This is where existing code does commit or abort...
        // }
        //
        // //TODO: 109642 return tiMessageResult;
        
        return new TIMessageResult();
    }

    private async Task<ImportResult> ImportObjectExceptionWrapper(ImportResult command,
        PlantScopedImportDataContext scopedImportDataContext, CancellationToken cancellationToken)
    {
        try
        {
            return await ImportObject(command, scopedImportDataContext, cancellationToken);
        }
        catch (Exception ex)
        {
            return command with { Errors = [..command.Errors, command.GetImportError(ex.Message)] };
        }
    }

    private async Task<ImportResult> ImportObject(ImportResult command, PlantScopedImportDataContext scopedImportDataContext, CancellationToken cancellationToken)
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

        //Import module is running as a background service which is lifetime singleton.
        //A singleton service cannot retrieve scoped services via constructor.
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var plantSetter = scope.ServiceProvider.GetRequiredService<IPlantSetter>();
        var mainApiAuthenticator = scope.ServiceProvider.GetRequiredService<IMainApiAuthenticator>();
        var currentUserSetter = scope.ServiceProvider.GetRequiredService<ICurrentUserSetter>();
        var claimsPrincipalProvider = scope.ServiceProvider.GetRequiredService<IClaimsPrincipalProvider>();
        var claimsTransformation = scope.ServiceProvider.GetService<IClaimsTransformation>();
        var authenticatorOptions = scope.ServiceProvider.GetService<IAuthenticatorOptions>();
        
        if (claimsTransformation is null)
        {
            throw new Exception("Could not get a valid ClaimsTransformation instance, value is null");
        }

        if (authenticatorOptions is null)
        {
            throw new Exception("Could not get a valid IAuthenticatorOptions instance, value is null");
        }

        plantSetter.SetPlant(scopedImportDataContext.Plant);

        mainApiAuthenticator.AuthenticationType = AuthenticationType.AsApplication;

        currentUserSetter.SetCurrentUserOid(Guid.Parse("5ea68d43-5ee1-4c28-9c79-bc54a004f269"));

        await AddOidClaimForCurrentUser(claimsPrincipalProvider, claimsTransformation, Guid.NewGuid());

        await mediator.Send(command.Command!, cancellationToken);

        //TODO: 106687 CommandFailureHandler;

        //TODO: 109642 return ImportResult.Ok();

        return command;
    }

    private async Task AddOidClaimForCurrentUser(IClaimsPrincipalProvider claimsPrincipalProvider, IClaimsTransformation claimsTransformation, Guid oid)
    {
        var currentUser = claimsPrincipalProvider.GetCurrentClaimsPrincipal();
        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaim(new Claim(ClaimsExtensions.Oid, oid.ToString()));
        currentUser.AddIdentity(claimsIdentity);

        await claimsTransformation.TransformAsync(currentUser);
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
