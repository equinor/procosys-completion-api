using System.Diagnostics;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;


namespace Equinor.ProCoSys.Completion.TieImport;

public sealed class ImportHandler(
    IPunchItemImportHandler punchItemImportHandler,
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

            
            var tiObject = mapped.Message!.Objects.First();
            CheckForScriptInjection(tiObject);
            ValidateTieObjectCommonMinimumRequirements(tiObject);
            var tiMessageResult = await punchItemImportHandler.ImportMessage(tiObject);
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

    //Should return true if method is  one of CREATE, INSERT or ALLOCATE
   
    
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
