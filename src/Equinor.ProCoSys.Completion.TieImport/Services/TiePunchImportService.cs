using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;
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
        
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var punchImportService = scope.ServiceProvider.GetRequiredService<IPunchItemImportService>();
        var importErrors = await punchImportService.HandlePunchImportMessageAsync(punchImportMessage);
        
        if(importErrors.Count != 0)
        {
            return CreateTiValidationErrorMessageResult(tiObject.Guid, importErrors);
        }
        
        var messageResult = CreateTiMessageSuccessResult(tiObject.Guid);
        return messageResult;
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

    private static TIMessageResult CreateTiMessageSuccessResult(Guid messageGuid)
    {
        var messageResult = new TIMessageResult
        {
            Result =  MessageResults.Successful,
            Guid = messageGuid
        };
        messageResult.AddLogEntry($"GUID '{messageGuid}' imported successfully", "PunchItem");
        return messageResult;
    }
}
