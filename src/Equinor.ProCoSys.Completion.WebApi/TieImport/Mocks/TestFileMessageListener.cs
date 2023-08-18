using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Config;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.WebApi.TieImport.Mocks;

/// <summary>
/// This class acts like a TIE simulator. It reads XML files from local disk, converts them to TIInterfaceMessages and sends them to the configured message handler.
/// An instance of the class is instantiated for each configured partition (site). The init code puts a reference to all files with site corresponding with this instance in a list to process.
/// The number of messages to send at each partitions interval is configured by the "TestFileChunkSize" setting.
/// When all files are processed, the instance/partition is disabled and will not handle more files while the app is running.
/// </summary>
public class TestFileMessageListener<TTie1AdapterConfig, TTie1AdapterPartitionConfig> : IMessageListener<TTie1AdapterConfig, TTie1AdapterPartitionConfig, Tie1Message, Tie1Receipt>
    where TTie1AdapterConfig : ITie1AdapterConfig<TTie1AdapterPartitionConfig>
    where TTie1AdapterPartitionConfig : ITie1AdapterPartitionConfig
{
    private readonly ILogger<TestFileMessageListener<TTie1AdapterConfig, TTie1AdapterPartitionConfig>> _logger;
    private readonly IOptions<TieImportOptions> _configOptions;

    private readonly List<string> _testFiles = new();

    public TestFileMessageListener(
        ILogger<TestFileMessageListener<TTie1AdapterConfig, TTie1AdapterPartitionConfig>> logger,
        IOptions<TieImportOptions> configOptions)
    {
        _logger = logger;
        _configOptions = configOptions;
    }

    public Task Init(TTie1AdapterConfig config, TTie1AdapterPartitionConfig partition)
    {
        var testMessageFiles = Directory.GetFiles(_configOptions.Value.TestFileMessageListenerPath);

        foreach (var file in testMessageFiles)
        {
            var message = ConvertFileToMessageObject(file);

            // add files to listener with site corresponding with current partition
            if (message.Site == partition.Key)
            {
                _testFiles.Add(file);
            }
        }

        _logger.LogInformation($"Found {_testFiles.Count} test files for site/partition {partition.Key}.");

        return Task.CompletedTask;
    }

    public Task<ListenResult<Tie1Message>> ListenForMessages(
        TTie1AdapterConfig config,
        TTie1AdapterPartitionConfig partition,
        CancellationToken stoppingToken)
    {
        if (!_testFiles.Any())
        {
            _logger.LogInformation($"No more files to process for site {partition.Key}. Disabling partition.");

            partition.Disabled = true;
            return Task.FromResult(ListenResult<Tie1Message>.CreateOk(new List<Tie1Message>())); // empty result
        }

        var tie1Messages = new List<Tie1Message>();
        var filesToProcess = _testFiles.Take(_configOptions.Value.TestFileChunkSize).ToList();

        foreach (var file in filesToProcess)
        {
            var message = ConvertFileToMessageObject(file);
            tie1Messages.Add(new Tie1Message { Message = message });

            _logger.LogInformation($"Sending message from file [{file}]. Message GUID={message.Guid} ({partition.Key})");

            // remove processed file from the list
            _testFiles.Remove(file);
        }

        return Task.FromResult(ListenResult<Tie1Message>.CreateOk(tie1Messages));
    }

    public Task<AckResult> Ack(Tie1Receipt receipt)
    {
        _logger.LogInformation(
            $"Receipt written to TestFileMessageListener. Message GUID={receipt.Message.Guid} ({receipt.Message.Site}). Status: {receipt.Status}. Comment: {receipt.Comment}");

        return Task.FromResult(AckResult.CreateOk());
    }

    private static TIInterfaceMessage ConvertFileToMessageObject(string filePath)
    {
        var xmlSerializer = new XmlSerializer(typeof(TIInterfaceMessage));

        using var streamReader = new StreamReader(filePath, Encoding.Default);
        var deserializedObject = xmlSerializer.Deserialize(streamReader) as TIInterfaceMessage;
        if (deserializedObject is null)
        {
            throw new ArgumentNullException(nameof(deserializedObject), $"Could not deserialize file {filePath}");
        }
        return deserializedObject;
    }

    public DateTime? PausedUntil { get; set; }
}
