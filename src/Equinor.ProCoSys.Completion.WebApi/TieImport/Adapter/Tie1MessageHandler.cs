using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.TieImport.Adapter;

public class Tie1MessageHandler : IMessageHandler<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>
{
    private readonly ILogger<Tie1MessageHandler> _logger;

    public Tie1MessageHandler(ILogger<Tie1MessageHandler> logger) 
        => _logger = logger;

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
