using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.TieImport.Adapter;

public class Tie1MessageHandler : IMessageHandler<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>
{
    private readonly ILogger<Tie1MessageHandler> _logger;
    private readonly IImportHandler _importHandler;

    public Tie1MessageHandler(ILogger<Tie1MessageHandler> logger, IImportHandler importHandler)
    {
        _logger = logger;
        _importHandler = importHandler;
    }

    public Task<MessageHandleResult<Tie1Receipt>> HandleSinglePerPartition(
        TieAdapterConfig config,
        TieAdapterPartitionConfig partition,
        Tie1Message message,
        CancellationToken stoppingToken)
        => HandleMessage(message);

    public Task<MessageHandleResult<Tie1Receipt>> HandleSingle(
        TieAdapterConfig config,
        Tie1Message message,
        CancellationToken stoppingToken)
        => HandleMessage(message);

    public Task Init(TieAdapterConfig config, TieAdapterPartitionConfig partition) => Task.CompletedTask;

    private async Task<MessageHandleResult<Tie1Receipt>> HandleMessage(Tie1Message message)
    {
        //TODO: 105593 Add custom application insights tracking 
        _logger.LogInformation("Got message with GUID={MessageGuid} ({MessageSite})", message.Message.Guid, message.Message.Site);

        await _importHandler.Handle(message.Message);
        
        //TODO: 105593 Add custom application insights tracking 

        return new MessageHandleResult<Tie1Receipt>
        {
            Receipt = Tie1Receipt.Create(message.Message, ReceiptStatus.Successful, "Always successful for now...")
        };
    }
}
