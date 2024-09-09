using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using Microsoft.Extensions.DependencyInjection;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Services;

public sealed class TiePunchImportService(IServiceScopeFactory serviceScopeFactory) : ITiePunchImportService
{
    public async Task<TIMessageResult> ImportMessage(TIObject tiObject)
    {
        var validationErrors = ValidateInput(tiObject);
        
        if (validationErrors.Count != 0)
        {
            return CreateTiValidationErrorMessageResult(tiObject.Guid, validationErrors);
        }

        var punchImportMessage = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);
        
        var importMessageErrors = ValidatePunchImportMessages(punchImportMessage);
        if (importMessageErrors.Count != 0)
        {
            return CreateTiValidationErrorMessageResult(punchImportMessage.MessageGuid, importMessageErrors);
        }
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var punchImportService = scope.ServiceProvider.GetRequiredService<IPunchItemImportService>();
        var importResult = await punchImportService.HandlePunchImportMessage(punchImportMessage);
        
        var messageResult = CreateTiMessageResult(tiObject.Guid, importResult.Errors.ToList());
        return messageResult;
    }

    private static List<ImportError> ValidatePunchImportMessages(PunchItemImportMessage punchImportMessage)
    {
        var commandValidator = new PunchItemImportMessageValidator();
        var validationResult = commandValidator.Validate(punchImportMessage);
        return validationResult.Errors.Select(e => punchImportMessage.ToImportError(e.ErrorMessage)).ToList();
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
}
