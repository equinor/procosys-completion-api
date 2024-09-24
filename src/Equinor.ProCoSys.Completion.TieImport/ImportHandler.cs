using System.Diagnostics;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Services;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class ImportHandler(
    ITiePunchImportService tiePunchImportService,
    IImportSchemaMapper importSchemaMapper,
    ILogger<ImportHandler> logger)
    : IImportHandler
{

    /// <summary>
    /// Does some simple input validation, maps the message using CommonLib importSchemaMapper.
    /// Then Handles Import of the PunchItem.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<TIMessageResult> Handle(TIInterfaceMessage message)
    {
        if (HasMoreThanOneObject(message, out var errorResponse))
        {
            return errorResponse;
        }
        
        logger.LogDebug("To import a message with name {ObjectName}, Class {ObjectClass}, Site {Site}",
            message.ObjectName, message.ObjectClass, message.Site);

        var sw = Stopwatch.StartNew();
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
            
            var tiObject = mapped.Message!.Objects.First();
            
            if (!IsCreateMethod(tiObject))
            {
                return new TIMessageResult
                {
                    Result = MessageResults.Failed,
                    ErrorMessage = "Only CREATE and INSERT methods are supported at this time"
                };
            }
            
            CheckForScriptInjection(tiObject);
            ValidateTieObjectCommonMinimumRequirements(tiObject);
       
            var tiMessageResult = await tiePunchImportService.ImportMessage(tiObject);
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

    private static bool HasMoreThanOneObject(TIInterfaceMessage message, out TIMessageResult response)
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

    private static bool IsCreateMethod(TIObject o) => o.Method == "CREATE";
    
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
}
