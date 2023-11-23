using System.Text;
using static Equinor.ProCoSys.Completion.DbSyncToPCS4.Column;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public class SyncUpdateHandler
    {
        private readonly IOracleDBExecutor _oracleDBExecutor;

        public SyncUpdateHandler(IOracleDBExecutor oracleDBExecutor) => _oracleDBExecutor = oracleDBExecutor;

        public async Task HandleAsync(object entity, SyncMappingConfig syncMappingConfig, CancellationToken cancellationToken = default)
        {
            var syncMappingList = syncMappingConfig._syncMappingList;

            var entityType = entity.GetType();
            var sourceTableName = entityType.Name.ToLower();

            var columnConfigs = syncMappingList.Where(config =>
                    config.SourceTable.Equals(sourceTableName)).ToDictionary(column => column.SourceColumn);

            if (columnConfigs.Count < 0)
            {
                throw new Exception("Map configuraiton is missing.");
            }

            var primaryKeyConfig = columnConfigs.Where(config => config.Value.IsPrimaryKey == true).Single().Value;
            var primaryKeyInfo = entityType.GetProperty(primaryKeyConfig.SourceType.ToString());

            if (primaryKeyInfo == null)
            {
                throw new Exception("Primary key not configured for ????");
            }

            var primaryKeyValue = primaryKeyInfo.GetValue(entity)?.ToString();

            if (primaryKeyValue == null)
            {
                throw new Exception("Value for primary key is missing for ????");
            }

            var columnsForUpdate = GetListWithColumnsForUpdate(entity, columnConfigs);

            var updateStatement = await BuildUpdateStatement(primaryKeyConfig, primaryKeyValue, columnConfigs, columnsForUpdate);

            await _oracleDBExecutor.ExecuteDBWrite(updateStatement);
        }

        private static List<Column> GetListWithColumnsForUpdate(object entity, Dictionary<string, ColumnSyncConfig> columnConfigs)
        {
            var columns = new List<Column>();

            foreach (var col in columnConfigs)
            {
                var keys = col.Key.Split('.');

                string? value = null;

                var property = entity.GetType().GetProperty(keys[0]);

                if (property == null)
                {
                    continue; //property is not found in entity, and is not included
                }

                var valueObject = property.GetValue(entity);

                if (keys.Length == 1)
                {
                    value = valueObject?.ToString();
                }
                else if (keys.Length == 2)
                {
                    var nestedProperty = valueObject?.GetType().GetProperty(keys[1]);
                    value = nestedProperty?.GetValue(valueObject)?.ToString();
                }
                else
                {
                    throw new Exception("Finding values in entity object for more than two levels is not supported");
                }

                var column = new Column()
                {
                    Name = col.Key,
                    Value = value,
                    Type = columnConfigs[col.Key].SourceType
                };
                columns.Add(column);
            }
            return columns;
        }

        private async Task<string> BuildUpdateStatement(ColumnSyncConfig primaryKeyConfig, string primaryKeyValue, Dictionary<string, ColumnSyncConfig> columnConfigs, List<Column> updateColumns)
        {
            var noOfRowsInTargetQuery = $"select count(*) from {primaryKeyConfig.TargetTable} where {primaryKeyConfig.TargetColumn} = {ValueConvertion.GetSqlParameterValue(primaryKeyValue, primaryKeyConfig.SourceType)}";

            var noOfRowsInTarget = await _oracleDBExecutor.ExecuteDBQueryCountingRows(noOfRowsInTargetQuery);

            if (noOfRowsInTarget != 1)
            {
                throw new Exception($"Number of rows should be 1, but was {noOfRowsInTarget}");
            }

            var targetTable = columnConfigs.First().Value.TargetTable;
            var updateStatement = new StringBuilder($"update {targetTable} set ");

            foreach (var column in updateColumns)
            {
                if (column.Name == primaryKeyConfig.SourceColumn)
                {
                    continue; //Skip primary key. Cannot be changed. 
                }

                var targetValue = await GetTargetValueForColumn(column, columnConfigs[column.Name]);

                updateStatement.Append($"{columnConfigs[column.Name].TargetColumn} = {targetValue}");

                if (column != updateColumns.Last())
                {
                    updateStatement.Append(", ");
                }
            }

            updateStatement.Append($" where {primaryKeyConfig.TargetColumn} = {ValueConvertion.GetSqlParameterValue(primaryKeyValue, primaryKeyConfig.SourceType)}");

            return updateStatement.ToString();
        }

        private async Task<string> GetTargetValueForColumn(Column column, ColumnSyncConfig columnConfig)
        {
            var sourceValue = column.Value;

            var convertionMethod = columnConfig.ValueConvertionMethod;
            if (convertionMethod != null && convertionMethod != "" && column.Value != null)
            {
                switch (convertionMethod)
                {
                    case "GuidToMainLibId":
                        return await ValueConvertion.GuidToMainLibId(column.Value, _oracleDBExecutor);
                }
            };

            return ValueConvertion.GetSqlParameterValue(sourceValue, column.Type);
        }


    }
}
