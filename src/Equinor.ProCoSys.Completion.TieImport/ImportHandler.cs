using System.Diagnostics;
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
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class ImportHandler(
    IServiceScopeFactory serviceScopeFactory,
    IImportSchemaMapper importSchemaMapper,
    ILogger<ImportHandler> logger)
    : IImportHandler
{
    public async Task<TIResponseFrame> Handle(TIInterfaceMessage? message)
    {
        var response = new TIResponseFrame();
        if (message is null)
        {
            logger.LogWarning("Received an empty message. Skipped");
            return response;
        }

        logger.LogInformation("To import a message with name {ObjectName}, Class {ObjectClass}, Site {Site}",
            message.ObjectName, message.ObjectClass, message.Site);

        var sw = new Stopwatch();
        sw.Start();

        TIMessageResult? tiMessageResult = null;
        try
        {
            //TODO: 106683 ObjectFixers

            var mapped = importSchemaMapper.Map(message);

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
        logger.LogCritical("Import elapsed {Elapsed}", sw.Elapsed);

        return response;
    }

    private async Task<TIMessageResult> ImportMessage(TIInterfaceMessage message)
    {
        logger.LogInformation("To import message GUID={MessageGuid} with {MessageCount} object(s)", message.Guid,
            message.Objects.Count);

        var importResults = CreateImportResults(message);
        using var scope = serviceScopeFactory.CreateScope();

        var scopedContext = await CreateScopedContexts(importResults);
        var tasks = CreateCommandTasks(importResults, scopedContext);

        var results = await Task.WhenAll(tasks);
        var messageResult = CreateTiMessageResult(message, results);

        return messageResult;
    }

    private static TIMessageResult CreateTiMessageResult(TIInterfaceMessage message, ImportResult[] results)
    {
        var messageResult = new TIMessageResult
        {
            Guid = message.Guid,
            Result = results.All(x => x.Errors.Length == 0)
                ? MessageResults.Successful
                : MessageResults.Failed,
            ErrorMessage = results.Any(x => x.Errors.Length != 0)
                ? "One or more objects failed to import"
                : string.Empty
        };

        foreach (var errorResult in results.Where(x => x.Errors.Length != 0))
        {
            foreach (var error in errorResult.Errors)
            {
                messageResult.AddLogEntry(new TILogEntry
                {
                    InterfaceName = "PunchItem",
                    LogDescription = error.ToString(),
                    Guid = Guid.NewGuid(),
                    LogScope = "General",
                    LogType = "Error",
                    TimeStamp = DateTime.UtcNow
                });
            }
        }

        foreach (var successResult in results.Where(x => x.Errors.Length == 0))
        {
            messageResult.AddLogEntry($"GUID '{successResult.TiObject.Guid}' imported successfully", "PunchItem");
        }

        return messageResult;
    }

    private IEnumerable<Task<ImportResult>> CreateCommandTasks(List<ImportResult> importResults, Dictionary<string, PlantScopedImportDataContext> scopedContext)
    {
        var tasks = importResults.GroupBy(x => x.Message?.Plant)
            .SelectMany(plantMessage =>
            {
                if (plantMessage.Key is null)
                {
                    return plantMessage
                        .Select(Task.FromResult);
                }

                var context = scopedContext[plantMessage.Key];
                var mapper = new PunchItemImportMessageCommandMapper(context);

                var commands = mapper
                    .Map(plantMessage.ToArray());


                return commands.Select(c => c.Errors.Length == 0
                    ? ImportObjectExceptionWrapper(c, context, CancellationToken.None)
                    : Task.FromResult(c));
            });
        return tasks;
    }

    private async Task<Dictionary<string, PlantScopedImportDataContext>> CreateScopedContexts(List<ImportResult> importResults)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var importDataFetcher = scope.ServiceProvider.GetRequiredService<IImportDataFetcher>();
        var tieOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<TieImportOptions>>();
        
        var contextBuilder = new PlantScopedImportDataContextBuilder(importDataFetcher, tieOptions);
        var scopedContext = await contextBuilder
            .BuildAsync(importResults
                .Where(x => x.Message is not null)
                .Select(x => x.Message!).ToArray(), CancellationToken.None);
        return scopedContext;
    }

    private static List<ImportResult> CreateImportResults(TIInterfaceMessage message)
    {
        var validator = new PunchTiObjectValidator();

        var importResults = message.Objects
            .Select(tiObject =>
            {
                var errors = validator
                    .Validate(tiObject)
                    .Errors
                    .Select(error => tiObject.ToImportError(error.ErrorMessage))
                    .ToArray();
                return new ImportResult(tiObject, null, null, errors);
            })
            .ToList()
            .Select(TiObjectToPunchItemImportMessage.ToPunchItemImportMessage)
            .ToList();
        return importResults;
    }

    private async Task<ImportResult> ImportObjectExceptionWrapper(ImportResult command,
        PlantScopedImportDataContext scopedImportDataContext, CancellationToken cancellationToken)
    {
        try
        {
            return await ImportObject(command, scopedImportDataContext, cancellationToken);
        }
        catch (ValidationException ve)
        {
            var validationErrors = ve.Errors
                .Select(x => command.GetImportError(x.ErrorMessage)).ToArray();
            return command with { Errors = [..command.Errors, ..validationErrors] };
        }
        catch (Exception ex)
        {
            return command with { Errors = [..command.Errors, command.GetImportError(ex.Message)] };
        }
    }

    private async Task<ImportResult> ImportObject(ImportResult importResult,
        PlantScopedImportDataContext scopedImportDataContext, CancellationToken cancellationToken)
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

        if (importResult.Errors.Length != 0 || importResult.Command is null)
        {
            logger.LogInformation("Not importing object with GUID={Guid} due to errors", importResult.TiObject.Guid);
            return importResult;
        }

        //Import module is running as a background service which is lifetime singleton.
        //A singleton service cannot retrieve scoped services via constructor.
        using var scope = serviceScopeFactory.CreateScope();
        var tieConfig = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<TieImportOptions>>();
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

        currentUserSetter.SetCurrentUserOid(scopedImportDataContext.Persons.First(x => x.UserName == tieConfig.CurrentValue.ImportUserName).Guid);

        await AddOidClaimForCurrentUser(claimsPrincipalProvider, claimsTransformation, Guid.NewGuid());

        await mediator.Send(importResult.Command, cancellationToken);

        //TODO: 106687 CommandFailureHandler;

        //TODO: 109642 return ImportResult.Ok();

        return importResult;
    }

    private async Task AddOidClaimForCurrentUser(IClaimsPrincipalProvider claimsPrincipalProvider,
        IClaimsTransformation claimsTransformation, Guid oid)
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
        logger.LogError(
            "Error when committing message. Exception: {ExceptionMessage} Stacktrace: {StackTrace} TIEMessage: {TieMessage}",
            e.Message, e.StackTrace, message.ToString());

        return tiMessageResult;
    }

    private static void AddResultOfImportOperationToResponseObject(TIInterfaceMessage message,
        TIMessageResult? tiMessageResult,
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
