using System.Text;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Oracle.ManagedDataAccess.Client;
using static Equinor.ProCoSys.Completion.DbSyncToPCS4.Column;

namespace Equinor.ProCoSys.Completion.DbSyncToPOCS4
{

    public class DbSynchronizer
    {
        public static void SetOracleConnection(string oracleConn) => DbConnStr = oracleConn;

        private static string? DbConnStr { get; set; }

        readonly static List<ColumnSyncConfig> _syncMapping = SyncMappingConfig.GetInstance().getSyncMappingList();

        public DbSynchronizer()
        {
            if (DbConnStr == null)
            {
                throw new Exception("DB connection string for Oracle is not set.");
            }
        }

        static public void SyncChangesToMain(object entity)
        {
            var entityType = entity.GetType();

            var sourceTableName = entityType.Name.ToLower();

            //Get config for columns
            var columnConfigs = _syncMapping.Where(config =>
                    config.SourceTable.Equals(sourceTableName)).ToDictionary(column => column.SourceColumn);

            if (columnConfigs.Count < 0)
            {
                throw new Exception("Map configuraiton is missing.");
            }

            // finn primary key
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

            //Lag liste med kolonner som skal oppdateres (source og value)
            var columns = new List<Column>();

            foreach (var col in columnConfigs)
            {
                var property = entityType.GetProperty(col.Key);

                if (property != null)
                {
                    var column = new Column()
                    {
                        Name = property.Name,
                        Value = property.GetValue(entity)?.ToString(),
                        Type = columnConfigs[property.Name].SourceType
                    };
                    columns.Add(column);
                }
            }

            var updateStatement = BuildUpdateStatement(primaryKeyConfig, primaryKeyValue, columnConfigs, columns);
            Console.WriteLine($"SQL update statement: {updateStatement}");
            ExecuteDBWrite(updateStatement);

        }


        private static string BuildUpdateStatement(ColumnSyncConfig primaryKeyConfig, string primaryKeyValue, Dictionary<string, ColumnSyncConfig> columnConfigs, List<Column> updateColumns)
        {

            //Verify that one and only one row exist in target db, for given primarykey
            var noOfRowsInTargetQuery = $"select count(*) from {primaryKeyConfig.TargetTable} where {primaryKeyConfig.TargetColumn} = {getSqlParameter(primaryKeyValue, primaryKeyConfig.SourceType)}";

            var noOfRowsInTarget = ExecuteDBQueryCountingRows(noOfRowsInTargetQuery);

            if (noOfRowsInTarget != 1)
            {
                throw new Exception($"Number of rows should be 1, but was {noOfRowsInTarget}");
            }

            //Create string for sql update statement
            var targetTable = columnConfigs.First().Value.TargetTable;
            var updateStatement = new StringBuilder($"update {targetTable} set ");

            foreach (var column in updateColumns)
            {
                //primary key must not be included (da må det være en bug et sted...)
                if (column.Name == primaryKeyConfig.SourceColumn)
                {
                    continue;
                }

                var value = GetValueForColumn(column, columnConfigs[column.Name]);

                updateStatement.Append($"{column.Name} = {value}");

                if (column != updateColumns.Last())
                {
                    updateStatement.Append(", ");
                }
            }

            updateStatement.Append($" where {primaryKeyConfig.TargetColumn} = {getSqlParameter(primaryKeyValue, primaryKeyConfig.SourceType)}");

            return updateStatement.ToString();
        }

        private static string GetValueForColumn(Column column, ColumnSyncConfig columnConfig)
        {
            var value = column.Value;

            var convertionMethod = columnConfig.ValueConvertionMethod;
            if (convertionMethod != null && column.Value != null)
            {
                switch (convertionMethod)
                {
                    case "ChecklistGuidToTagCheckId":
                        value = ValueConvertion.GuidToMainLibId(column.Value, DbConnStr);
                        break;
                }
            };

            return getSqlParameter(value, column.Type);
        }


        public static string getSqlParameter(string? value, DataType type)
        {
            switch (type)
            {
                case DataType.String:
                    return $"'{value}'";
                case DataType.Int:
                    return value ?? ""; //todo
                case DataType.Date:
                    return $"'{value}'"; //todo
                case DataType.Guid:
                    return value == null ? "''" : $"'{value.Replace("-", string.Empty).ToUpper()}'";
                default:
                    throw new NotImplementedException();
            }
        }


        private static void ExecuteDBWrite(string sqlStatement)
        {
            using (OracleConnection connection = new OracleConnection(DbConnStr))
            {
                connection.Open();

                using (OracleCommand cmd = new OracleCommand(sqlStatement, connection))
                {
                    //todo: Bør bruke Parameters.Add, for unngå sql injection
                    int rowsAffected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} rows affected.");

                    connection.Close();
                }
            }
        }

        private static int ExecuteDBQueryCountingRows(string sqlQuery)
        {
            using (OracleConnection connection = new OracleConnection(DbConnStr))
            {
                connection.Open();

                using (OracleCommand command = new OracleCommand(sqlQuery, connection))
                {
                    //todo: Bør bruke Parameters.Add, for unngå sql injection
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return Convert.ToInt32(command.ExecuteScalar());
                        }
                    }
                }
                return -1;
            }
        }
    }
}

