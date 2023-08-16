using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;
//using AdapterConsoleApp.ApplicationInsights;
//using CommonServiceLocator;
//using Import.ProCoSys;

namespace Equinor.ProCoSys.Completion.WebApi.TieImport.Adapter;

public class Tie1MessageHandler : IMessageHandler<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>
{
    private readonly ILogger<Tie1MessageHandler> _logger;
    //private readonly ITelemetryHelper _telemetryHelper;
    //private readonly IImportHandler _importHandler;

    private readonly Stopwatch _processingTimeStopwatch = new Stopwatch();

    public Tie1MessageHandler(
        ILogger<Tie1MessageHandler> logger
        //ITelemetryHelper telemetryHelper
    )
    {
        _logger = logger;
        //_telemetryHelper = telemetryHelper;
        //_importHandler = ServiceLocator.Current.GetInstance<IImportHandler>();
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
        _processingTimeStopwatch.Restart();

        //_telemetryHelper.TrackMessageReceivedEvent(message.Message);
        _logger.LogInformation($"Got message with GUID={message.Message.Guid} ({message.Message.Site})");

        // Pass message to the internal import handler logic.
        //var responseFrame = _importHandler.Handle(WrapInMessageFrame(message));


        // Get the handling result (there will only ever by one result as we are handling messages one by one).
        //var result = responseFrame.Results.First();

        _processingTimeStopwatch.Stop();
        //_telemetryHelper.TrackMessageProcessedEvent(
        //    message.Message,
        //    GetReceiptStatus(result).ToString(),
        //    _processingTimeStopwatch.ElapsedMilliseconds,
        //    result.Logs);

        // Create and return receipt.
        //var receipt = Tie1Receipt.Create(message.Message, GetReceiptStatus(result), GetReceiptComment(result));
        //receipt.Logs = GetReceiptLogs(result, message.Message.Site);

        //return new MessageHandleResult<Tie1Receipt>
        //{
        //    Receipt = receipt
        //};

        return new MessageHandleResult<Tie1Receipt>
        {
                
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

    private static List<Log> GetReceiptLogs(TIMessageResult result, string site)
    {
        var logs = new List<Log>
        {
            // default log statement
            new Log
            {
                LogType = LogTypes.Information,
                Text = $"Processed by the adapter handling site {site}."
            }
        };

        if (result.Logs.Any())
        {
            // We will only return the first 100 log entries due to TIE performance issues.
            const int LogCount = 100;

            logs.AddRange(
                result.Logs.Take(LogCount).Select(logEntry =>
                    new Log
                    {
                        LogType = ConvertToEnumLogType(logEntry.LogType),
                        Text = logEntry.LogDescription
                    }));

            if (result.Logs.Count > LogCount)
            {
                logs.Add(new Log
                {
                    LogType = LogTypes.Information,
                    Text = $"{result.Logs.Count - LogCount} more log entries were truncated."
                });
            }
        }

        return logs;
    }

    private static bool ResultHasWarnings(TIMessageResult result)
        => result.Logs.Any(log => ConvertToEnumLogType(log.LogType) == LogTypes.Warning);

    private static string GetWarningsFromResult(TIMessageResult result)
    {
        if (!ResultHasWarnings(result))
        {
            return "No warnings found.";
        }

        var warningLogs = result.Logs
            .Where(log => ConvertToEnumLogType(log.LogType) == LogTypes.Warning)
            .Select(log => log.LogDescription);

        return string.Join(" | ", warningLogs);
    }

    private static LogTypes ConvertToEnumLogType(string logType)
        => Enum.TryParse(logType, true, out LogTypes parsedLogType)
            ? parsedLogType
            : LogTypes.Unknown;

    private static TIMessageFrame WrapInMessageFrame(Tie1Message message)
        => new TIMessageFrame {Messages = new List<TIInterfaceMessage> {message.Message}};
}
