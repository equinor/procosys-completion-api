using System.Diagnostics;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Equinor.ProCoSys.Completion.TieImport.Services;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class ImportHandler(
    IServiceScopeFactory serviceScopeFactory,
    IImportSchemaMapper importSchemaMapper,
    ILogger<ImportHandler> logger)
    : IImportHandler
{
    public async Task<TIMessageResult> Handle(TIInterfaceMessage message)
    {
        if (MessageNotImportable(message, out var responseFrame))
        {
            return responseFrame;
        }

        logger.LogInformation("To import a message with name {ObjectName}, Class {ObjectClass}, Site {Site}",
            message.ObjectName, message.ObjectClass, message.Site);

        var sw = new Stopwatch();
        sw.Start();
        try
        {
            var mapped = importSchemaMapper.Map(message);
            if (mapped.ErrorResult is not null)
            {
                return mapped.ErrorResult;
                
            }

            if (mapped.Message?.Objects.FirstOrDefault() is null)
            {
                return CreateTIMessageErrorResult(message, "No objects in message");
            }

            var tiMessageResult = await ImportMessage(mapped.Message!.Objects.First());
            tiMessageResult.Guid = message.Guid;
            tiMessageResult.ExternalReference = message.ExternalReference;
            return tiMessageResult;

        }
        catch (Exception e)
        {
            var errorResult = e.ToMessageResult();
            errorResult.Guid = message.Guid;
            errorResult.ExternalReference = message.ExternalReference;
            logger.LogError(
                "Error when committing message. Exception: {ExceptionMessage} Stacktrace: {StackTrace} TIEMessage: {TieMessage}",
                e.Message, e.StackTrace, message.ToString());
            return errorResult;
        }
        finally
        {
            sw.Stop();
            logger.LogInformation("Import elapsed {Elapsed}", sw.Elapsed);
        }
    }

    private static TIMessageResult CreateTIMessageErrorResult(TIInterfaceMessage message, string errorMessage) =>
        new()
        {
            Guid = message.Guid,
            ExternalReference = message.ExternalReference,
            Result = MessageResults.Failed,
            ErrorMessage = errorMessage
        };

    private static bool MessageNotImportable(TIInterfaceMessage message, out TIMessageResult response)
    {
        response = new TIMessageResult();
        switch (message.Objects.Count)
        {
            // Observe: We only support one object per message.
            case > 1:
                response= CreateTIMessageErrorResult(message, "Only one object per message is supported"); 
                return true;
            case 0: 
                response = CreateTIMessageErrorResult(message, "No objects in message");
            return true;
        }

        return false;
    }

    private async Task<TIMessageResult> ImportMessage(TIObject message)
    {
        CheckForScriptInjection(message);
        ValidateTieObjectCommonMinimumRequirements(message);
        if (!IsCreateMethod(message))
        {
            return new TIMessageResult
            {
                Result = MessageResults.Failed,
                ErrorMessage = "Only CREATE and INSERT methods are supported at this time"
            };
        }
        
        var validationErrors = ValidateInput(message);
        
        if (validationErrors.Count != 0)
        {
            return CreateTiValidationErrorMessageResult(message.Guid, validationErrors);
        }

        var punchImportMessage = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(message);
        
        var importMessageErrors = ValidatePunchImportMessages(punchImportMessage);
        if (importMessageErrors.Count != 0)
        {
            return CreateTiValidationErrorMessageResult(punchImportMessage.MessageGuid, importMessageErrors);
        }

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var punchImportService = scope.ServiceProvider.GetRequiredService<IPunchImportService>();
        var importResult = await punchImportService.HandlePunchImportMessage(punchImportMessage);
        
        var messageResult = CreateTiMessageResult(message.Guid, importResult.Errors.ToList());
        return messageResult;
    }
    
    //Should return true if method is  one of CREATE, INSERT or ALLOCATE
    private static bool IsCreateMethod(TIObject o) =>
        o.Method?.ToUpperInvariant() == "CREATE" 
          || o.Method?.ToUpperInvariant() == "INSERT" 
          || o.Method?.ToUpperInvariant() == "ALLOCATE";

    private static List<ImportError> ValidatePunchImportMessages(PunchItemImportMessage punchImportMessage)
    {
            var commandValidator = new PunchItemImportMessageValidator();
            var validationResult = commandValidator.Validate(punchImportMessage);
            return  validationResult.Errors.Select(e => punchImportMessage.ToImportError(e.ErrorMessage)).ToList();
    }

    private static List<ImportError> ValidateInput(TIObject message)
    {
        var validator = new PunchTiObjectValidator();
        var errors = validator
                        .Validate(message)
                        .Errors
                        .Select(error => message.ToImportError(error.ErrorMessage));
        
        return errors.ToList();
    }

   
    private static TIMessageResult CreateTiValidationErrorMessageResult(Guid messageGuid, List<ImportError> errors)
    {
        var messageResult = new TIMessageResult
        {
            Guid = messageGuid,
            Result = MessageResults.Failed,
            ErrorMessage = $"Errors(1 of {errors.Count}): {errors.FirstOrDefault()?.Message}"
               
        };
        foreach (var error in errors)
        {
            messageResult.AddLogEntry(new TILogEntry
            {
                LogDescription = error.ToString(),
                Guid = Guid.NewGuid(),
                LogScopeByEnum = LogScopes.General,
                LogTypeByEnum = LogTypes.Error,
                TimeStamp = DateTime.UtcNow,
            });
        }
        return messageResult;
    }

    private static TIMessageResult CreateTiMessageResult(Guid messageGuid, List<ImportError> resultErrors)
    {
        
        var messageResult = new TIMessageResult
        {
            Result = resultErrors.Count != 0
                ? MessageResults.Failed
                : MessageResults.Successful,
            ErrorMessage = resultErrors.Count != 0
                ? $"Errors(1 of {resultErrors.Count}): {resultErrors.FirstOrDefault()?.Message}"
                : string.Empty
        };

        foreach (var error in resultErrors)
        {
                messageResult.AddLogEntry(new TILogEntry
                {
                    LogDescription = error.ToString(),
                    Guid = Guid.NewGuid(),
                    LogScope = "General",
                    LogType = "Error",
                    TimeStamp = DateTime.UtcNow
                });
        }

        if (resultErrors.Count == 0)
        {
            messageResult.AddLogEntry($"GUID '{messageGuid}' imported successfully", "PunchItem");
        }
        return messageResult;
    }
    
    private static void CheckForScriptInjection(TIObject tieObject)
    {
        // Run through object attributes and make sure that no strings contains HTML Script tags.
        if (tieObject.Attributes == null)
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
