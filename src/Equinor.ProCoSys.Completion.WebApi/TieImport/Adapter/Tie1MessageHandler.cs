﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.TieImport.Adapter;

public class Tie1MessageHandler : IMessageHandler<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>
{
    private readonly ILogger<Tie1MessageHandler> _logger;
    private readonly IImportSchemaMapper _commonLibMapper;

    public Tie1MessageHandler(ILogger<Tie1MessageHandler> logger, IImportSchemaMapper commonLibMapper)
    {
        _logger = logger;
        _commonLibMapper = commonLibMapper;
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
}