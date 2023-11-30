using System.Text;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    /**
     * Executes the an update of a row in the PCS 4 database, based on a sourceObject and mapping configuration
     */
    public class SyncUpdateHandler
    {
        private readonly IOracleDBExecutor _oracleDBExecutor;
        public SyncUpdateHandler(IOracleDBExecutor oracleDBExecutor) => _oracleDBExecutor = oracleDBExecutor;

        /**
         * Handle the syncronization
         */
        public async Task<string> BuildSqlUpdateStatementAsync(string sourceObjectName, object sourceObject, ISyncMappingConfig syncMappingConfig, CancellationToken cancellationToken = default)
        {
            var syncMappings = syncMappingConfig.GetSyncMappingsForSourceObject(sourceObjectName);

            //Find the primary key
            var primaryKeyConfig = syncMappings.Where(config => config.IsPrimaryKey == true).SingleOrDefault();

            if (primaryKeyConfig == null)
            {
                throw new Exception($"The configuration should have a primary key property defined. Source object name: {sourceObjectName}");
            }

            var primaryKeyValue = GetSourcePropertyValue(primaryKeyConfig, sourceObject);
            var primaryKeySqlParameterValue = await ValueConversion.GetSqlParameterValueAsync(primaryKeyValue, primaryKeyConfig, _oracleDBExecutor, cancellationToken);

            var columnsForUpdate = await GetTargetUpdatesAsync(sourceObject, syncMappings, cancellationToken);
            return GetUpdateStatement(primaryKeyConfig, primaryKeySqlParameterValue, columnsForUpdate);
        }


        /**
         * Creates a list with updates the be executed on the target database. 
         */
        private async Task<List<ColumnUpdate>> GetTargetUpdatesAsync(object sourceObject, List<ColumnSyncConfig> syncMappingList, CancellationToken cancellationToken)
        {
            var columns = new List<ColumnUpdate>();

            foreach (var col in syncMappingList)
            {
                if (col.IsPrimaryKey)
                {
                    continue;
                }

                // Find value for source property 
                var sourcePropertyValue = GetSourcePropertyValue(col, sourceObject);

                var targetColumnValue = await ValueConversion.GetSqlParameterValueAsync(sourcePropertyValue, col, _oracleDBExecutor, cancellationToken);

                var columnUpdate = new ColumnUpdate(col.TargetColumn, targetColumnValue);

                columns.Add(columnUpdate);
            }
            return columns;
        }

        /**
         * Will find the value on the property in the source object. 
         * This value might be in a nested property (e.g ActionBy.Oid)
         */
        private static object? GetSourcePropertyValue(ColumnSyncConfig column, object sourceObject)
        {
            var sourcePropertyNameParts = column.SourceProperty.Split('.');
            if (sourcePropertyNameParts.Length > 2)
            {
                throw new Exception($"Only one nested level is supported for entities, so {column.SourceObjectName}.{column.SourceProperty} is not supported.");
            }

            var sourcePropertyName = sourcePropertyNameParts[0];
            var sourceProperty = sourceObject.GetType().GetProperty(sourcePropertyName);

            if (sourceProperty == null)
            {
                throw new Exception($"A property in configuration is missing in source object: {column.SourceObjectName}.{column.SourceProperty}");
            }

            var sourcePropertyValue = sourceProperty.GetValue(sourceObject);

            if (sourcePropertyValue != null && sourcePropertyNameParts.Length > 1)
            {
                //We must find the nested property
                sourceProperty = sourcePropertyValue?.GetType().GetProperty(sourcePropertyNameParts[1]);

                if (sourceProperty == null)
                {
                    throw new Exception($"A nested property in configuration is missing in source object: {column.SourceObjectName}.{column.SourceProperty}");
                }

                sourcePropertyValue = sourceProperty.GetValue(sourcePropertyValue);
            }

            return sourcePropertyValue;
        }

        /**
         * Build a string that gives the update statement
         */
        private static string GetUpdateStatement(ColumnSyncConfig primaryKeyConfig, string primaryKeySqlParamValue, List<ColumnUpdate> updateColumns)
        {
            var updateStatement = new StringBuilder($"update {primaryKeyConfig.TargetTable} set ");

            foreach (var column in updateColumns)
            {
                updateStatement.Append($"{column.TargetColumnName} = {column.TargetColumnValue}");

                if (column != updateColumns.Last())
                {
                    updateStatement.Append(", ");
                }
            }

            //Todo: Vi vil her lage en dictionary med parameterverdier, som senere brukes når vi lager en oracle command.

            updateStatement.Append($" where {primaryKeyConfig.TargetColumn} = {primaryKeySqlParamValue}");

            return updateStatement.ToString();
        }
    }
}
