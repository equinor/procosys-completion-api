using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Equinor.TI.TIE.Adapter.Base.Message;

namespace AdapterConsoleApp.Configuration
{
    public class ConfigurationOptions
    {
        public string AdapterTieUri { get; set; } = string.Empty;
        public string AzureClientId { get; set; } = string.Empty;
        public string AzureTenantId { get; set; } = string.Empty;
        public string AzureTieApiId { get; set; } = string.Empty;
        public string AzureKeyVaultUrl { get; set; } = string.Empty;
        public string AzureCertificateName { get; set; } = string.Empty;

        /// <summary>
        /// See usage in <see cref="AdapterMessageHandleBehavior"/>.
        /// </summary>
        public bool AdapterParallelMessageHandling { get; set; }

        /// <summary>
        /// Number of messages to handle in each batch from TIE.
        /// </summary>
        public int AdapterMessageChunkSize { get; set; }

        /// <summary>
        /// Pause interval (in milliseconds) between each batch of messages. See <see cref="AdapterMessageChunkSize"/>.
        /// </summary>
        public int AdapterIdleTimeBetweenBatch { get; set; }

        /// <summary>
        /// Pause interval (in milliseconds) between checking for new messages.
        /// </summary>
        public int AdapterIdleTimeOnNoMessages { get; set; }

        /// <summary>
        /// TIE application for source system message routing. See <see cref="AdapterApplication"/>.
        /// </summary>
        public string AdapterApplication { get; set; } = string.Empty;

        /// <summary>
        /// Comma-separated list of TIE site names to handle. See <see cref="AdapterPartitions"/>.
        /// </summary>
        public string AdapterSites { get; set; } = string.Empty;

        /// <summary>
        /// PerMessagePerPartition: run handle logic per message per listener/partition (parallel).
        /// FanIn: use if parallel handling is not possible.
        /// </summary>
        public MessageHandleBehavior AdapterMessageHandleBehavior => AdapterParallelMessageHandling
            ? MessageHandleBehavior.PerMessagePerPartition
            : MessageHandleBehavior.FanIn;

        /// <summary>
        /// List of partition configurations to handle (= TIE sites/plants).
        /// </summary>
        public List<TieAdapterPartitionConfig> AdapterPartitions =>
            string.IsNullOrEmpty(AdapterSites)
                ? new List<TieAdapterPartitionConfig>()
                : AdapterSites.Split(',').Select(site => new TieAdapterPartitionConfig {Key = site}).ToList();

        public bool TestEnableMockTie1Listener { get; set; }

        public bool TestEnableMockTie1MessageHandler { get; set; }

        public bool TestEnableTestFileMessageListener { get; set; }

        public string TestFileMessageListenerPath { get; set; } = string.Empty;

        public int TestFileChunkSize { get; set; }

        public override string ToString()
        {
            var newLine = Environment.NewLine;

            var options = $"TIE: {AdapterTieUri}{newLine}" +
                $"MessageHandleBehavior: {AdapterMessageHandleBehavior}{newLine}" +
                $"MessageChunkSize: {AdapterMessageChunkSize}{newLine}" +
                $"IdleTimeBetweenBatch: {TimeSpan.FromMilliseconds(AdapterIdleTimeBetweenBatch).TotalSeconds} (seconds){newLine}" +
                $"IdleTimeOnNoMessages: {TimeSpan.FromMilliseconds(AdapterIdleTimeOnNoMessages).TotalSeconds} (seconds){newLine}" +
                $"Application: {AdapterApplication}{newLine}" +
                $"Sites: {AdapterSites}";

            if (TestEnableMockTie1Listener || TestEnableMockTie1MessageHandler || TestEnableTestFileMessageListener)
            {
                options += $"{newLine}*** Test settings ***{newLine}" +
                    $"EnableMockTie1Listener: {TestEnableMockTie1Listener}{newLine}" +
                    $"EnableMockTie1MessageHandler: {TestEnableMockTie1MessageHandler}{newLine}" +
                    $"EnableTestFileMessageListener: {TestEnableTestFileMessageListener}{newLine}" +
                    $"TestFileMessageListenerPath: {TestFileMessageListenerPath}{newLine}" +
                    $"TestFileChunkSize: {TestFileChunkSize}";
            }

            return options;
        }
    }
}
