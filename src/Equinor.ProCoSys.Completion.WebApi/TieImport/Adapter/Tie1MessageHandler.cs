using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PersonCommands.CreatePerson;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Converters;
using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.WebApi.TieImport.Adapter;

public class Tie1MessageHandler : IMessageHandler<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>
{
    private readonly ILogger<Tie1MessageHandler> _logger;
    private readonly IImportSchemaMapper _commonLibMapper;
    private readonly IMessageInspector _messageInspector;

    public Tie1MessageHandler(ILogger<Tie1MessageHandler> logger, IImportSchemaMapper commonLibMapper, IMessageInspector messageInspector)
    {
        _logger = logger;
        _commonLibMapper = commonLibMapper;
        _messageInspector = messageInspector;
    }

    public Task<MessageHandleResult<Tie1Receipt>> HandleSinglePerPartition(
        TieAdapterConfig config,
        TieAdapterPartitionConfig partition,
        Tie1Message message,
        CancellationToken stoppingToken)
        => Task.FromResult(HandleMessage(message));

    public Task<MessageHandleResult<Tie1Receipt>> HandleSingle(
        TieAdapterConfig config,
        Tie1Message message,
        CancellationToken stoppingToken)
        => Task.FromResult(HandleMessage(message));

    public Task Init(TieAdapterConfig config, TieAdapterPartitionConfig partition) => Task.CompletedTask;

    private MessageHandleResult<Tie1Receipt> HandleMessage(Tie1Message message)
    {
        //TODO: 105593 Add custom application insights tracking 
        //_telemetryHelper.TrackMessageReceivedEvent(message.Message);
        _logger.LogInformation($"Got message with GUID={message.Message.Guid} ({message.Message.Site})");

        //TODO: Route message to handling code and obtain a result from the handling
        var result = _commonLibMapper.Map(message.Message);
        if (result.Message is null)
        {
            //TODO: What to return here??
            return new MessageHandleResult<Tie1Receipt>();
        }

        foreach (var tiObject in result.Message.Objects)
        {
            _logger.LogInformation(tiObject.GetLogFriendlyString() + ": Starting import of object.");
            //TODO: Remember ProCoSys custom mapping TieProCoSysMapper
            //TODO: Include CheckForScriptInjection??
            //TODO: Consider where to call ValidateTieObjectCommonMinimumRequirements
            //TODO: Remember sitespecific handling
            var pcsObject = CreatePcsDtoFromTieImport(tiObject, out var importResult);
            //TODO: JSOI Null check
            var incomingObjectType = pcsObject!.GetType();
            //var pcsCommand = CreateCommand(incomingObjectType, pcsObject.ImportMethod);
            //var createPunchItemCommand = CreatePunchItemCommand(pcsObject);
            if (importResult is not null)
            {
                // Abort, no further processing of objects.
                //TODO: JSOI Fix
                return new MessageHandleResult<Tie1Receipt>
                {
                    //Empty for now
                };
                //return importResult;
            }

            PreparePcsObject(message.Message, tiObject, pcsObject);

            //TODO: Consider below code
            //var commandResult = LoadObject(pcsObject);
            //if (commandResult == null || !commandResult.Success)
            //{
            //    return _commandFailureHandler.HandleFailureResult(commandResult);
            //}

            //TODO: JSOI Fix
            return new MessageHandleResult<Tie1Receipt>
            {
                //Empty for now
            };
            //return ImportResult.Ok();
        }
        //TODO: 105593 Add custom application insights tracking 
        //_telemetryHelper.TrackMessageProcessedEvent(
        //    message.Message,
        //    GetReceiptStatus(result).ToString(),
        //    _processingTimeStopwatch.ElapsedMilliseconds,
        //    result.Logs);

        return new MessageHandleResult<Tie1Receipt>
        {
            //Empty for now
        };
    }

    //private CreatePunchItemCommand CreatePunchItemCommand(IPcsObjectIn pcsObject)
    //{
    //    var punchItemFromTie = pcsObject as PcsPunchItemIn;
      
    //    var createPunchItemCommand = new CreatePunchItemCommand(punchItemFromTie.Description, projectGuid,
    //        raisedByOrgGuid, clearingByOrgGuid, priorityGuid, sortingGuid, typeGuid);
    //}

    private void PreparePcsObject(TIInterfaceMessage message, TIObject tieObject, IPcsObjectIn pcsObject)
    {
        // Assign the method on the pcsObject.
        TIEPCSCommonConverters.FillInCommandVerbToPerformFromTieObject(
            tieObject,
            message,
            pcsObject);

        // Get and translate what to do (MODIFY, DELETE etc), this is generic for any type.
        // This comes from the object or message header.
        // Set its ImportOptions.
        _messageInspector.UpdateImportOptions(pcsObject, message);

        // Make sure that Name is set on input on object for further logging purposes.
        if (string.IsNullOrWhiteSpace(tieObject.ObjectName))
        {
            tieObject.ObjectName = ((PcsaObjectIn)pcsObject).Name;
        }

        // Do eventual custom preparations of the object before shipping it to ProCoSys.
        //TODO: JSOI Consider lines below
        //try
        //{
        //    TIEProCoSysImportCustomImport.CustomImport(pcsObject, tieObject, message);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError($"CustomImport failed: {ex.Message}", ex);
        //}
    }

    private IPcsObjectIn? CreatePcsDtoFromTieImport(TIObject tieObject, out ImportResult? importResult)
    {
        importResult = null;
        var pcsObject = ConvertTieObjectToPcsDto(tieObject);

        if (pcsObject is not null)
        {
            SetPlantIdFromTieObject(tieObject, pcsObject);

            //TODO: JSOI Consider lines below
            //if (Logger.IsDebugEnabled)
            //{
            //    var tieClassificationInfo = "";
            //    if (!string.IsNullOrWhiteSpace(tieObject.Classification))
            //    {
            //        tieClassificationInfo = $"({tieObject.Classification.ToUpper()})";
            //    }

            //    var debugInfo =
            //        $"Made ready for import: Site: {tieObject.Site}, {tieObject.ObjectClass.ToUpper()} {tieClassificationInfo} {pcsObject.Name}";
            //    Logger.Debug(debugInfo);
            //}

            return pcsObject;
        }

        var message = $"Import of Class {tieObject.ObjectClass}";
        if (!string.IsNullOrWhiteSpace(tieObject.Classification))
        {
            message += $", Classification {tieObject.Classification}";
        }

        message += " is not supported.";
        _logger.LogError(message);
        importResult = ImportResult.SingleError(message);
        return null;
    }

    private void SetPlantIdFromTieObject(TIObject tieObject, IPcsObjectIn pcsObject)
    {
        if (tieObject.Site == null)
        {
            return;
        }

        var pcsObjectType = pcsObject.GetType();
        var pcsProperty = pcsObjectType.GetProperty(
            "PlantId",
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (pcsProperty != null)
        {
            pcsProperty.SetValue(pcsObject, tieObject.Site, null);
        }

        //TODO: Remember Ncr special handling
    }

    private IPcsObjectIn? ConvertTieObjectToPcsDto(TIObject tieObject) =>
        tieObject.ObjectClass.ToUpper() switch
        {
            "PUNCHITEM" => TIE2PCSPunchItemConverter.AssignPunchItemObject(tieObject),
            _ => null
        };
}
