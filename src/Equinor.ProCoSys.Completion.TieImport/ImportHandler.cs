using System.Diagnostics;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ValidationException = FluentValidation.ValidationException;

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
            var mapped = importSchemaMapper.Map(message);
            if (mapped.Message is null)
            {
                tiMessageResult = mapped.ErrorResult;
                return response;
            }
            tiMessageResult = await ImportMessage(mapped.Message!);
        }
        catch (Exception e)
        {
            tiMessageResult = CreateMessageResultFromException(message, e);
            logger.LogError(
                "Error when committing message. Exception: {ExceptionMessage} Stacktrace: {StackTrace} TIEMessage: {TieMessage}",
                e.Message, e.StackTrace, message.ToString());
        }
        finally
        {
            AddResultOfImportOperationToResponseObject(message, tiMessageResult, response);
        }

        sw.Stop();
        logger.LogInformation("Import elapsed {Elapsed}", sw.Elapsed);

        return response;
    }

    private async Task<TIMessageResult> ImportMessage(TIInterfaceMessage message)
    {
        var plant = message.Site;
        logger.LogInformation("To import message GUID={MessageGuid} with {MessageCount} object(s)", message.Guid,
            message.Objects.Count);
 
        var validationErrors = ValidateInput(message);
        if (validationErrors.SelectMany(ve => ve.errors).ToList().Count != 0)
        {
            return CreateTiValidationErrorMessageResult(message, validationErrors);
        }
        
        var punchImportMessages = MapToPunchImportMessages(message).ToList();
        
        //Creates a dictionary of data based on the plant of the message.
        var scopedContexts = await CreateScopedContexts(punchImportMessages);
        
        var importMessageErrors = ValidateBasedOnFetchedData(punchImportMessages, scopedContexts, plant);
        if (importMessageErrors.SelectMany(ve => ve.errors).ToList().Count != 0)
        {
            return CreateTiValidationErrorMessageResult(message, importMessageErrors);
        }

        var resultTasks = punchImportMessages.Select(pim 
            => HandlePunchImportMessage(pim, scopedContexts[plant]));
        
        var results = await Task.WhenAll(resultTasks);
        var messageResult = CreateTiMessageResult(message, results);

        return messageResult;
    }

    private static List<(Guid guid, IEnumerable<ImportError> errors)> ValidateBasedOnFetchedData(List<PunchItemImportMessage> punchImportMessages, Dictionary<string, PlantScopedImportDataContext> scopedContexts, string plant)
    {
        var importMessageErrors = new List<(Guid, IEnumerable<ImportError>)>();
        punchImportMessages.ForEach(pim =>
        {
            var commandValidator = new PunchItemImportMessageValidator(scopedContexts[plant]);
            var validationResult = commandValidator.Validate(pim);
            if (validationResult.IsValid)
            {
                return;
            }
            var guidAndErrors =(pim.TiObject.Guid, validationResult.Errors.Select(e => pim.ToImportError(e.ErrorMessage)));
            importMessageErrors.Add(guidAndErrors);

        });
        return importMessageErrors;
    }

    private static List<(Guid Guid, IEnumerable<ImportError> errors)> ValidateInput(TIInterfaceMessage message)
    {
        var validator = new PunchTiObjectValidator();

        var validationErrors =
            message.Objects
                .Select(tiObject =>
                {
                    var errors = validator
                        .Validate(tiObject)
                        .Errors
                        .Select(error => tiObject.ToImportError(error.ErrorMessage));
                    return (tiObject.Guid,errors);
                }).ToList();
        return validationErrors;
    }

    private async Task<ImportResult> HandlePunchImportMessage(PunchItemImportMessage message,
        PlantScopedImportDataContext scopedContext)
    {
        var referencesService = new CommandReferencesService(scopedContext);
        List<ImportError> errors = [];
        object? command;
        switch (message.TiObject.Method)
        {
             //same as create
            case "CREATE": case "ALLOCATE": case "INSERT":
                {
                    var (createCommand, importErrors) = GetAndValidateCreateCommand(message, referencesService);
                    errors = importErrors.ToList();
                    command = createCommand;
                    break;
                }
            case "MODIFY": case "APPEND":
                {
                    var punchItem = referencesService.GetPunchItem(message);
                    if(punchItem is null)
                    {
                        var (createCommand, importErrors) = GetAndValidateCreateCommand(message, referencesService);
                        errors = importErrors.ToList();
                        command = createCommand;
                    }
                    else
                    {
                        var (updateCommand, importErrors) = GetAndValidateUpdateCommand(message,punchItem, referencesService);
                        errors =  importErrors;
                        command = updateCommand;
                    }
                    break; 
                }
            case "UPDATE":
                {
                    var punchItem = referencesService.GetPunchItem(message);
                    if(punchItem is null)
                    {
                        errors = [message.ToImportError("PunchItem not found, can't be updated")];
                        return new ImportResult(message.TiObject, message, errors);
                    }
                    var (updateCommand, importErrors) = GetAndValidateUpdateCommand(message,punchItem, referencesService);
                    errors = importErrors;
                    command = updateCommand;
                    break;
                }
            case "DELETE":
                {
                    var punchItem = referencesService.GetPunchItem(message);
                    if(punchItem is null)
                    {
                        errors = [message.ToImportError("PunchItem not found, can't be deleted")];
                        return new ImportResult(message.TiObject, message, errors);
                    }
                    command = new DeletePunchItemCommand(punchItem.Guid,
                        punchItem.RowVersion.ConvertToString());
                    break;
                }
            default:
                throw new NotImplementedException(); 
        }
        
        if (errors.Count != 0)
        {
            return new ImportResult(message.TiObject, message, errors);
        }
       

        var importResult = new ImportResult(message.TiObject,message,[]);
        try
        { 
            CheckForScriptInjection(message.TiObject);
            ValidateTieObjectCommonMinimumRequirements(message.TiObject);
            await ImportObject(command!, scopedContext, CancellationToken.None);
            return importResult;

        }
        catch (ValidationException ve)
        {
            var validationErrors = ve.Errors
                .Select(x => importResult.GetImportError(x.ErrorMessage)).ToArray();
            return importResult with { Errors = [..importResult.Errors, ..validationErrors] };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"ImportObject failed unexpectedly");
            return importResult with { Errors = [..importResult.Errors, importResult.GetImportError(ex.Message)] };
        }
    }

    private static (ImportUpdatePunchItemCommand? updateCommand, List<ImportError>) GetAndValidateUpdateCommand(PunchItemImportMessage message,PunchItem punchItem,  CommandReferencesService referencesService)
    {
        var references = referencesService.GetCreatePunchItemReferences(message);
        if(references.Errors.Length != 0)
        {
            return (null, references.Errors.ToList());
        }
        var patchDocument = PunchItemImportMessageToUpdateCommand.CreateJsonPatchDocument(message, punchItem, references);
        var clearedBy = references.ClearedBy;
        var verifiedBy = references.VerifiedBy;
        var rejectedBy = references.RejectedBy;
        var category = message.Category;

        var command = new ImportUpdatePunchItemCommand(
            message.TiObject.Guid,
            references.ProjectGuid,
             message.TiObject.Site,
            punchItem.Guid,
            patchDocument,
            category,
            clearedBy,
            verifiedBy,
            rejectedBy,
            punchItem.RowVersion.ConvertToString());
        return (command, []);
        
    }

    private static (CreatePunchItemCommand?, IEnumerable<ImportError>) GetAndValidateCreateCommand(PunchItemImportMessage message, CommandReferencesService referencesService)
    {
        var references = referencesService.GetCreatePunchItemReferences(message);
        if(references.Errors.Length != 0)
        {
            return (null, references.Errors);
        }
        
        var command = new CreatePunchItemCommandForImport(
            message.Category!.Value,
            message.Description.Value ?? string.Empty,
            references.CheckListGuid,
            references.RaisedByOrgGuid,
            references.ClearedByOrgGuid,
            null,
            message.DueDate.Value,
            null,
            null,
            references.TypeGuid,
            null,
            null,
            null,
            null,
            null,
            message.ExternalPunchItemNo,
            false, // FIXME
            message.MaterialEta.Value,
            message.MaterialNo.Value
        );
        return (command,references.Errors);
    }
    
    private static TIMessageResult CreateTiValidationErrorMessageResult(TIInterfaceMessage message, IEnumerable<(Guid TiObjectGuid, IEnumerable<ImportError> errors)> objectErrors)
    {
        var messageResult = new TIMessageResult
        {
            Guid = message.Guid,
            Result = MessageResults.Failed,
            ErrorMessage = "One or more objects failed to import"
               
        };
        foreach (var errorByObject in objectErrors)
        {
            foreach (var error in errorByObject.errors)
            {
                messageResult.AddLogEntry(new TILogEntry
                {
                    InterfaceName = "PunchItem",
                    LogDescription = error.ToString(),
                    Guid = errorByObject.TiObjectGuid,
                    LogScope = "General",
                    LogType = "Error",
                    TimeStamp = DateTime.UtcNow
                });
            }
        }
        return messageResult;
    }

    private static TIMessageResult CreateTiMessageResult(TIInterfaceMessage message, ImportResult[] results)
    {
        var messageResult = new TIMessageResult
        {
            Guid = message.Guid,
            Result = results.All(x => !x.Errors.Any())
                ? MessageResults.Successful
                : MessageResults.Failed,
            ErrorMessage = results.Any(x => x.Errors.Any())
                ? "One or more objects failed to import"
                : string.Empty
        };

        foreach (var errorResult in results.Where(x => x.Errors.Any()))
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

        foreach (var successResult in results.Where(x => !x.Errors.Any()))
        {
            messageResult.AddLogEntry($"GUID '{successResult.TiObject.Guid}' imported successfully", "PunchItem");
        }

        return messageResult;
    }

    // private IEnumerable<Task<ImportResult>> CreateCommandTasks(List<PunchItemImportMessage> importResults, Dictionary<string, PlantScopedImportDataContext> scopedContext)
    // {
    //     var tasks = importResults.GroupBy(x => x.TiObject.Site)
    //         .SelectMany(grouping =>
    //         {
    //             if (grouping.Key is null)
    //             {
    //                 return grouping
    //                     .Select(Task.FromResult);
    //             }
    //
    //             var context = scopedContext[grouping.Key];
    //             var mapper = new PunchItemImportMessageCommandMapper(context);
    //
    //             var importResultsWithCommands = mapper
    //                 .Map(grouping.ToArray());
    //
    //
    //             return importResultsWithCommands.Select(c => c.Errors.Length == 0
    //                 ? ImportObjectExceptionWrapper(c, context, CancellationToken.None)
    //                 : Task.FromResult(c));
    //         });
    //     return tasks;
    // }

    private async Task<Dictionary<string, PlantScopedImportDataContext>> CreateScopedContexts(IEnumerable<PunchItemImportMessage> punchItemImportMessages)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var importDataFetcher = scope.ServiceProvider.GetRequiredService<IImportDataFetcher>();
        var tieOptions = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<TieImportOptions>>();
        
        var contextBuilder = new PlantScopedImportDataContextBuilder(importDataFetcher, tieOptions);
        var scopedContext = await contextBuilder
            .BuildAsync(punchItemImportMessages.ToList(), CancellationToken.None);
        return scopedContext;
    }

    private static IEnumerable<PunchItemImportMessage> MapToPunchImportMessages(TIInterfaceMessage message) 
        => message.Objects.Select(TiObjectToPunchItemImportMessage.ToPunchItemImportMessage);
    
    private async Task ImportObject(object importResult,
        PlantScopedImportDataContext scopedImportDataContext, CancellationToken cancellationToken)
    {
        //Import module is running as a background service which is lifetime singleton.
        //A singleton service cannot retrieve scoped services via constructor.
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var plantSetter = scope.ServiceProvider.GetRequiredService<IPlantSetter>();
        var claimsTransformation = scope.ServiceProvider.GetService<IClaimsTransformation>();
        var tieConfig = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<TieImportOptions>>();
        var currentUserSetter = scope.ServiceProvider.GetRequiredService<ICurrentUserSetter>();
        
        if (claimsTransformation is null)
        {
            throw new Exception("Could not get a valid ClaimsTransformation instance, value is null");
        }
        plantSetter.SetPlant(scopedImportDataContext.Plant);
        var importUser = scopedImportDataContext.Persons.First(x => x.UserName == tieConfig.CurrentValue.ImportUserName);
        currentUserSetter.SetCurrentUserOid(importUser.Guid);
       await mediator.Send(importResult, cancellationToken);
    }
    
    private static void CheckForScriptInjection(TIObject tieObject)
    {
        // Run through object attributes and make sure that no strings contains HTML Script tags.
        if (tieObject?.Attributes == null)
        {
            return;
        }

        foreach (var attributeValue in tieObject.Attributes
                     .Select(a => a.Value)
                     .Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            if (attributeValue.Contains("<SCRIPT ", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("Error: Security. Script injection attempted: " + attributeValue);
            }
        }
    }
    
    private static void ValidateTieObjectCommonMinimumRequirements(TIObject tieObject)
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
          throw new Exception(message);
        }
    }

    // private async Task SetScopeAuthorizationForCommand(ImportResult importResult,
    //     PlantScopedImportDataContext scopedImportDataContext, IOptionsMonitor<TieImportOptions> tieConfig,
    //     ICurrentUserSetter currentUserSetter, IClaimsPrincipalProvider claimsPrincipalProvider,
    //     IClaimsTransformation claimsTransformation)
    // {
    //     var importUser = scopedImportDataContext.Persons.First(x => x.UserName == tieConfig.CurrentValue.ImportUserName);
    //     currentUserSetter.SetCurrentUserOid(importUser.Guid);
    //
    //     var projectGuid = Guid.Empty;
    //     if (importResult.Command is CreatePunchItemCommand pc)
    //     {
    //         projectGuid = pc.CheckListDetailsDto.ProjectGuid;
    //     }
    //     
    //     await AddOidClaimForCurrentUser(claimsPrincipalProvider, claimsTransformation, importUser.Guid, projectGuid, importResult.Message!.Responsible);
    // }

    // private async Task AddOidClaimForCurrentUser(IClaimsPrincipalProvider claimsPrincipalProvider,
    //     IClaimsTransformation claimsTransformation, Guid oid, Guid projectGuid, string responsible)
    // {
    //     var currentUser = claimsPrincipalProvider.GetCurrentClaimsPrincipal();
    //     var claimsIdentity = new ClaimsIdentity();
    //     claimsIdentity.AddClaim(new Claim(ClaimsExtensions.Oid, oid.ToString()));
    //     claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, ClaimsTransformation.GetProjectClaimValue(projectGuid)));
    //     claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, ClaimsTransformation.GetRestrictionRoleClaimValue(responsible)));
    //     currentUser.AddIdentity(claimsIdentity);
    //
    //     await claimsTransformation.TransformAsync(currentUser);
    // }


    private TIMessageResult CreateMessageResultFromException(TIInterfaceMessage message, Exception e)
    {
        var exceptionResult = e.ToMessageResult();
        return exceptionResult;
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
