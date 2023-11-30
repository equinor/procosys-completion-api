using System.Data;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    /**
     * Holds the configuration of the synchronization mapping. 
     * The data for the configuration is given by the SyncMappingConfig.csv file found in this folder.
     * The will be a source object comming from PCS 5, that maps to a target table in PCS 4. 
     * Each source object must have 'one' property configured, that is to be used as primary key against the PCS 4 database. 
     */
    public class SyncMappingConfig
    {
        public List<ColumnSyncConfig> _syncMappings;

        public SyncMappingConfig()
        {
            _syncMappings = new List<ColumnSyncConfig>();
            LoadMappingConfiguration();
        }

        public List<ColumnSyncConfig> GetSyncMappingsForSourceObject(string sourceObjectName)
        {
            var list = _syncMappings.Where(config =>
                config.SourceObjectName.Equals(sourceObjectName)).ToList();

            if (list.Count < 1)
            {
                throw new Exception($"Synchronization mapping is missing for source object {sourceObjectName}");
            }

            return list;
        }

        /**
         * Loads data from the mapping configuration file given as SyncMappingConfig.csv the the same folder
         */
        private void LoadMappingConfiguration()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SyncMappingConfig.csv");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The CSV file for synchronization mapping configuration was not found.");
            }

            try
            {
                var rows = File.ReadAllLines(filePath);

                for (var i = 1; i < rows.Length; i++)
                {
                    if (rows[i].StartsWith("-"))
                    {
                        continue; //Skip rows that starts with '-'. 
                    }

                    var columns = rows[i].Split(';', StringSplitOptions.TrimEntries);

                    if (columns.Length < 6 || columns.Length > 8)
                    {
                        throw new Exception($"The row contains the wrong number of columns, {rows[i]}.");
                    }

                    var sourceObjectName = columns[0];
                    var sourceProperty = columns[1];
                    var targetTable = columns[2];
                    var targetColumn = columns[3];

                    if (!bool.TryParse(columns[4], out var isPrimaryKey))
                    {
                        throw new Exception($"The isPrimaryKey is wrong on row {rows[i]}.");
                    }

                    if (!Enum.TryParse(columns[5], out DataColumnType sourceType))
                    {
                        throw new Exception($"The source type '{columns[5]}' is not supported on row {rows[i]}.");
                    }

                    var valueConversionMethod = columns[6] == null || columns[6].Length == 0 ? null : columns[6];

                    var config = new ColumnSyncConfig(
                        sourceObjectName,
                        sourceProperty,
                        targetTable,
                        targetColumn,
                        isPrimaryKey,
                        sourceType,
                        valueConversionMethod);

                    _syncMappings.Add(config);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred when reading sync mapping configuration.", ex);
            }
        }
    }
}
