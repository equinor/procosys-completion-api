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

        public enum DataColumnType
        {
            String,
            Int,
            DateTime,
            Bool,
            Guid
        }

        public List<ColumnSyncConfig> GetSyncMappingsForSourceObject(string sourceObjectName)
        {
            var list = _syncMappings.Where(config =>
                config.SourceObjectName.Equals(sourceObjectName)).ToList();

            if (list.Count < 0)
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
                    var columns = rows[i].Split(';', StringSplitOptions.TrimEntries);

                    if (columns.Length < 6 || columns.Length > 8)
                    {
                        throw new Exception($"The row contains the wrong number of columns, {rows[i]}.");
                    }

                    if (!bool.TryParse(columns[4], out var isPrimaryKey))
                    {
                        throw new Exception($"The isPrimaryKey is wrong on row {rows[i]}.");
                    }

                    if (!Enum.TryParse(columns[5], out DataColumnType sourceType))
                    {
                        throw new Exception($"The isPrimaryKey is wrong on row {rows[i]}.");
                    }

                    var config = new ColumnSyncConfig()
                    {
                        SourceObjectName = columns[0],
                        SourceProperty = columns[1],
                        TargetTable = columns[2],
                        TargetColumn = columns[3],
                        IsPrimaryKey = isPrimaryKey,
                        SourceType = sourceType,
                        ValueConvertionMethod = columns[6] == null || columns[6].Length == 0 ? null : columns[6]
                    };

                    _syncMappings.Add(config);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occured when reading sync mapping configuration.", ex);
            }
        }
    }
}
