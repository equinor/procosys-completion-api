namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    /**
     * Holds the configuration of the synchronization mapping. 
     * The data for the configuration is given by the SyncMappingConfig.csv file found in this folder.
     */
    public class SyncMappingConfig
    {
        public List<ColumnSyncConfig> _syncMappingList;

        public SyncMappingConfig()
        {
            _syncMappingList = new List<ColumnSyncConfig>();
            LoadMappingConfiguration();
        }

        public enum DataColumnType
        {
            String,
            Int,
            DateTime,
            Bool,
            Guid
        }

        public List<ColumnSyncConfig> GetSyncMappingListForTableName(string sourceTableName)
        {
            var list = _syncMappingList.Where(config =>
                config.SourceTable.Equals(sourceTableName)).ToList();

            if (list.Count < 0)
            {
                throw new Exception($"Synchronization mapping is missing for source table {sourceTableName}");
            }

            return list;
        }

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
                    SourceType = (DataColumnType)Enum.Parse(typeof(DataColumnType), columns[5]),
                    ValueConvertionMethod = columns[6] == null || columns[6].Length == 0 ? null: columns[6]
                };

                _syncMappingList.Add(config);
            }
        }
    }
}
