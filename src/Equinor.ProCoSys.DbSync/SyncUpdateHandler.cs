using System.Text;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public class SyncUpdateHandler
    {
        private readonly IOracleDBExecutor _oracleDBExecutor;

        public SyncUpdateHandler(IOracleDBExecutor oracleDBExecutor) => _oracleDBExecutor = oracleDBExecutor;

        public async Task HandleAsync(object entity, SyncMappingConfig syncMappingConfig, CancellationToken cancellationToken = default)
        {
            var entityType = entity.GetType();
            var sourceTableName = entityType.Name.ToLower();

            var syncMappingList = syncMappingConfig.GetSyncMappingListForTableName(sourceTableName);

            var primaryKeyConfig = syncMappingList.Where(config => config.IsPrimaryKey == true).Single();
            var primaryKeyValue = GetPrimaryKeyValue(entity, primaryKeyConfig);

            var primaryKeySqlParameterValue = await ValueConvertion.GetSqlParameterValue(primaryKeyValue, primaryKeyConfig, _oracleDBExecutor);

            await VerifyExistanceOfTargetRow(primaryKeyConfig, primaryKeySqlParameterValue);

            var columnsForUpdate = await GetListWithColumnsForUpdate(entity, syncMappingList);
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

        private async Task<List<ColumnUpdate>> GetListWithColumnsForUpdate(object entity, List<ColumnSyncConfig> syncMappingList)
        {
            var columns = new List<ColumnUpdate>();

            foreach (var col in syncMappingList)
            {
                if ( col.IsPrimaryKey)
                {
                    continue;
                }

                // Find source value object
                var sourceColumnNameParts = col.SourceColumn.Split('.');
                if (sourceColumnNameParts.Length > 2)
                {
                    throw new Exception($"Only one nested level is supported for entities, so {col.SourceType}.{col.SourceColumn} is not supported.");
                }

                var sourcePropertyName = sourceColumnNameParts[0];
                var sourceProperty = entity.GetType().GetProperty(sourcePropertyName);

                if (sourceProperty == null)
                {
                    continue;
                }

                var sourceValueObject = sourceProperty.GetValue(entity);

                if (sourceValueObject != null)
                {

                    if (sourceColumnNameParts.Length > 1)
                    {
                        //Find nested value object
                        var nestedProperty = sourceValueObject?.GetType().GetProperty(sourceColumnNameParts[1]);

                        if (nestedProperty == null)
                        {
                            throw new Exception($"Nested property was not found on entity for property {col.SourceColumn}");
                        }
                        sourceValueObject = nestedProperty?.GetValue(sourceValueObject);
                    }
                }

                var targetValue = await ValueConvertion.GetSqlParameterValue(sourceValueObject, col, _oracleDBExecutor);

                var columnUpdate = new ColumnUpdate()
                {
                    TargetColumnName = col.TargetColumn,
                    TargetValue = targetValue
                };

                columns.Add(columnUpdate);
            }
            return columns;
        }


        private static string BuildUpdateStatement(ColumnSyncConfig primaryKeyConfig, string primaryKeySqlParamValue, List<ColumnUpdate> updateColumns)
        {
            var updateStatement = new StringBuilder($"update {primaryKeyConfig.TargetTable} set ");

            foreach (var column in updateColumns)
            {
                updateStatement.Append($"{column.TargetColumnName} = {column.TargetValue}");

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
