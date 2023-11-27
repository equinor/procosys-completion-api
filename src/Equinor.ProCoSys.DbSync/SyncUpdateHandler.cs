using System.Data.Common;
using System.Text;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public class SyncUpdateHandler
    {
        private readonly IOracleDBExecutor _oracleDBExecutor;

        public SyncUpdateHandler(IOracleDBExecutor oracleDBExecutor) => _oracleDBExecutor = oracleDBExecutor;

        public async Task HandleAsync(object sourceObject, SyncMappingConfig syncMappingConfig, CancellationToken cancellationToken = default)
        {
            var sourceObjectType = sourceObject.GetType();
            var sourceObjectName = sourceObjectType.Name.ToLower();

            var syncMappings = syncMappingConfig.GetSyncMappingsForSourceObject(sourceObjectName);

            var primaryKeyConfig = syncMappings.Where(config => config.IsPrimaryKey == true).Single();
            var primaryKeyValue = GetPrimaryKeyValue(sourceObject, primaryKeyConfig);
            var primaryKeySqlParameterValue = await ValueConvertion.GetSqlParameterValue(primaryKeyValue, primaryKeyConfig, _oracleDBExecutor);

            await VerifyExistanceOfTargetRow(primaryKeyConfig, primaryKeySqlParameterValue);

            var columnsForUpdate = await GetTargetUpdates(sourceObject, syncMappings);
            var sqlUpdateStatement = BuildUpdateStatement(primaryKeyConfig, primaryKeySqlParameterValue, columnsForUpdate);

            await _oracleDBExecutor.ExecuteDBWrite(sqlUpdateStatement);
        }

        private static object GetPrimaryKeyValue(object entity, ColumnSyncConfig primaryKeyConfig)
        {
            var entityType = entity.GetType();
            var primaryKeyInfo = entityType.GetProperty(primaryKeyConfig.SourceType.ToString());

            if (primaryKeyInfo == null)
            {
                throw new Exception("Primary key not configured for ????");
            }

            var primaryKeyValue = primaryKeyInfo.GetValue(entity);

            if (primaryKeyValue == null)
            {
                throw new Exception("Value for primary key is missing for ????");
            }

            return primaryKeyValue;
        }

        /**
         * Creates a list with updates the be executed on the target database. 
         */
        private async Task<List<ColumnUpdate>> GetTargetUpdates(object sourceObject, List<ColumnSyncConfig> syncMappingList)
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

                if (sourcePropertyValue == null)
                {
                    continue; //property is not found in the source object, so we just skip this (todo: or throw exception?)
                }

                var targetColumnValue = await ValueConvertion.GetSqlParameterValue(sourcePropertyValue, col, _oracleDBExecutor);

                var columnUpdate = new ColumnUpdate()
                {
                    TargetColumnName = col.TargetColumn,
                    TargetColumnValue = targetColumnValue
                };

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


        private static string BuildUpdateStatement(ColumnSyncConfig primaryKeyConfig, string primaryKeySqlParamValue, List<ColumnUpdate> updateColumns)
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

            updateStatement.Append($" where {primaryKeyConfig.TargetColumn} = {primaryKeySqlParamValue}");

            return updateStatement.ToString();
        }

        private async Task VerifyExistanceOfTargetRow(ColumnSyncConfig primaryKeyConfig, string primaryKeySqlParameterValue)
        {
            var noOfRowsInTargetQuery = $"select count(*) from {primaryKeyConfig.TargetTable} where {primaryKeyConfig.TargetColumn} = {primaryKeySqlParameterValue}";

            var noOfRowsInTarget = await _oracleDBExecutor.ExecuteDBQueryCountingRows(noOfRowsInTargetQuery);

            if (noOfRowsInTarget != 1)
            {
                throw new Exception($"Number of rows should be 1, but was {noOfRowsInTarget}");
            }
        }
    }
}
