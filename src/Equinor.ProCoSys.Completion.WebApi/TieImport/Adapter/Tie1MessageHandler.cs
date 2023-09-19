using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.TieImport;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.WebApi.TieImport.Adapter;

public class Tie1MessageHandler : IMessageHandler<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>
{
    private readonly ILogger<Tie1MessageHandler> _logger;
    private readonly IImportSchemaMapper _commonLibMapper;
    private readonly IMessageInspector _messageInspector;
    private readonly IImportHandler _importHandler;

    public Tie1MessageHandler(ILogger<Tie1MessageHandler> logger, IImportSchemaMapper commonLibMapper, IMessageInspector messageInspector, IImportHandler importHandler)
    {
        _logger = logger;
        _commonLibMapper = commonLibMapper;
        _messageInspector = messageInspector;
        _importHandler = importHandler;
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

        
        
        //---------Core code
        //TODO: get rid of below line, and refactor ImportHandler.Handle to take a TIInterfaceMessage instead of TIMessageFrame
        //var tiMessageFrame = WrapInMessageFrame(message);
        //Makes sense that if return value is TIResponseFrame, the input parameter is TIMessageFrame
        //TIResponseFrame contains a collection of TIMessageResults
        var responseFrame = _importHandler.Handle(message.Message);

        // Get the handling result (there will only ever by one result as we are handling messages one by one).
        var result = responseFrame.Results.First();
        //TODO: Use result when creating receipt

        //--------End core code



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
    private static TIMessageFrame WrapInMessageFrame(Tie1Message message)
        => new TIMessageFrame { Messages = new List<TIInterfaceMessage> { message.Message } };
}
