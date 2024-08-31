using Equinor.ProCoSys.Completion.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Adapter;

public sealed class Tie1MessageHandler(ILogger<Tie1MessageHandler> logger, IImportHandler importHandler)
    : IMessageHandler<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>
{
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
        logger.LogInformation("Got message with GUID={MessageGuid} ({MessageSite})", message.Message.Guid,
            message.Message.Site);

        if (message.Message.ObjectType != "PUNCHITEM")
        {
            return new MessageHandleResult<Tie1Receipt>
            {
                Receipt = Tie1Receipt.Create(message.Message, ReceiptStatus.Filtered, "Invalid object type.")
            };
        }

        var response = await importHandler.Handle(message.Message);
        var result = response.Results.Single();

        //TODO: 105593 Add custom application insights tracking 

        return new MessageHandleResult<Tie1Receipt>
        {
            Receipt = Tie1Receipt.Create(message.Message, GetReceiptStatus(result), GetReceiptComment(result))
        };
    }

    private static ReceiptStatus GetReceiptStatus(TIMessageResult result)
    {
        if (result.Result == MessageResults.Successful)
        {
            return ResultHasWarnings(result) ? ReceiptStatus.Warning : ReceiptStatus.Successful;
        }

        return ReceiptStatus.Failed;
    }

    private static bool ResultHasWarnings(TIMessageResult result)
        => result.Logs.Any(log => log.LogType == LogTypes.Warning.ToString());
    
    private static string GetReceiptComment(TIMessageResult result)
    {
        switch (result.Result)
        {
            case MessageResults.Successful:
                {
                    return ResultHasWarnings(result)
                        ? $"Message processed OK with warnings: {GetWarningsFromResult(result)}"
                        : "Message processed OK.";
                }
            case MessageResults.Failed:
                return string.IsNullOrEmpty(result.ErrorMessage) ? "Failed. See log(s)." : result.ErrorMessage;
            default:
                return "Message processed.";
        }
    }
    
    private static string GetWarningsFromResult(TIMessageResult result)
    {
        if (!ResultHasWarnings(result))
        {
            return "No warnings found.";
        }

        var warningLogs = result.Logs
            .Where(log => log.LogType == LogTypes.Warning.ToString())
            .Select(log => log.LogDescription);

        return string.Join(" | ", warningLogs);
    }
}
