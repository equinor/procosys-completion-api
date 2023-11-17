using static Equinor.ProCoSys.Completion.DbSyncToPCS4.Column;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{

    /**
     * Singleton that holds the configuration of the synchronization mapping. 
     * The instance will be created at startup. When the mapping is updated, the system will need to be resarted.
     * The data for the configuration is given by the SyncMappingConfig.csv file found in this folder.
     */
    public sealed class SyncMappingConfig
    {
        private static readonly SyncMappingConfig s_instance = new();

        private readonly List<ColumnSyncConfig> _syncMapping;

        private SyncMappingConfig()
        {
            _syncMapping = new List<ColumnSyncConfig>();
            LoadMappingConfiguration();
        }

        public static SyncMappingConfig GetInstance() => s_instance;

        public List<ColumnSyncConfig> getSyncMappingList() => _syncMapping;

        private void LoadMappingConfiguration()
        {
            var fileName = "SyncMappingConfig.csv";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The CSV file for synchrnoization mapping configuration was not found.");
            }

            var rows = File.ReadAllLines(filePath);

            for (var i = 1; i < rows.Length; i++)
            {
                var columns = rows[i].Split(';', StringSplitOptions.TrimEntries);

                var config = new ColumnSyncConfig()
                {
                    SourceTable = columns[0],
                    SourceColumn = columns[1],
                    TargetTable = columns[2],
                    TargetColumn = columns[3],
                    IsPrimaryKey = bool.Parse(columns[4]),
                    SourceType = (DataType)Enum.Parse(typeof(DataType), columns[5]),
                    ValueConvertionMethod = columns[6]
                };

                _syncMapping.Add(config);
            }
        }
    }
}
