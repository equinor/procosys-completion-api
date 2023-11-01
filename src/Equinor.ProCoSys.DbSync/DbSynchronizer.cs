using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace Equinor.ProCoSys.DbSyncPOC
{

    public class DbSynchronizer
    {

        string connectionString = "User Id=PROCOSYS_REGRESSION_DEV1;Password=Ioyfgybpr1sXH6UyzcEpAYpY;Data Source=localhost:1521/ORCLPDB1;";

        List<ColumnSyncConfig> _syncConfig = new List<ColumnSyncConfig>();

        public DbSynchronizer()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../SyncConfig_test.csv");

            string[] lines = File.ReadAllLines(filePath);
            for (var i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(',');
                var config = new ColumnSyncConfig() { SourceTable = columns[0], TargetTable = columns[1], SourceColumn = columns[2], TargetColumn = columns[3], IsPrimaryKey = bool.Parse(columns[4]) };
                _syncConfig.Add(config);
            }
        }

        public void HandleEvent(SyncEvent syncEvent)
        {
            switch (syncEvent.Operation)
            {
                case SyncEvent.OperationType.Insert:
                    HandleInsertEvent(syncEvent);
                    break;
                case SyncEvent.OperationType.Update:
                    HandleUpdateEvent(syncEvent);
                    break;
                default:
                    throw new NotImplementedException($"SyncEvent with operation type {syncEvent.Operation} is not implemented.");
            }
        }

        private void HandleInsertEvent(SyncEvent syncEvent)
        {
            //todo: kast exception hvis vi mangler config for en kolonne
            string insertStatement = BuildInsertStatement(syncEvent);
            Console.WriteLine($"SQL insert statement: {insertStatement}");
            ExecuteDBWrite(insertStatement);
        }

        private void HandleUpdateEvent(SyncEvent syncEvent)
        {
            //todo: kast exception hvis vi mangler config for en kolonne
            string updateStatement = BuildUpdateStatement(syncEvent);
            Console.WriteLine($"SQL update statement: {updateStatement}");
            ExecuteDBWrite(updateStatement);
        }

        private string BuildInsertStatement(SyncEvent syncEvent)
        {
            //Get config for columns
            var columnConfigs = _syncConfig.Where(config =>
                    config.SourceTable.Equals(syncEvent.Table.ToLower())).ToDictionary(column => column.SourceColumn);

            if (columnConfigs.Count < 0)
            {
                throw new Exception("Map configuraiton is missing.");
            }

            //Todo: Verifisere om raden finnes fra før, og evt. gi exception. 

            var targetTable = columnConfigs.First().Value.TargetTable;

            //Create insert statement
            var insertStatement = new StringBuilder($"insert into {targetTable} (");

            foreach (var column in syncEvent.Columns)
            {
                insertStatement.Append(column.Name);

                if (column != syncEvent.Columns.Last())
                {
                    insertStatement.Append(", ");
                }
            }

            insertStatement.Append(") values (");

            foreach (var column in syncEvent.Columns)
            {

                //todo: Flytt type inn i config og bort fra syncevent. Lage en metode som returnerer string basert på config og column.

                var parameterValueStr = GetSqlParameter(column, columnConfigs[column.Name]);
                insertStatement.Append($"{parameterValueStr}");

                if (column != syncEvent.Columns.Last())
                {
                    insertStatement.Append(", ");
                }
            }

            insertStatement.Append(')');

            return insertStatement.ToString();
        }

        private string BuildUpdateStatement(SyncEvent syncEvent)
        {


            //Get config for columns
            var columnConfigs = _syncConfig.Where(config =>
                    config.SourceTable.Equals(syncEvent.Table.ToLower())).ToDictionary(column => column.SourceColumn);

            if (columnConfigs.Count < 0)
            {
                throw new Exception("Map configuraiton is missing.");
            }

            //find primary key 
            var primaryKeyConfig = columnConfigs.Where(config => config.Value.IsPrimaryKey == true).Single().Value;
            var primaryKey = syncEvent.Columns.Where(column => column.Name == primaryKeyConfig.SourceColumn).Single();

            string primaryKeyValue = GetSqlParameter(primaryKey, primaryKeyConfig);

            //Verify that one and only one row exist in target db, for given primarykey
            var noOfRowsInTargetQuery = $"select count(*) from {primaryKeyConfig.TargetTable} where {primaryKeyConfig.TargetColumn} = {primaryKeyValue}";

            var noOfRowsInTarget = ExecuteDBQueryCountingRows(noOfRowsInTargetQuery);

            if (noOfRowsInTarget != 1)
            {
                throw new Exception($"Number of rows should be 1, but was {noOfRowsInTarget}");
            }

            //Create string for sql update statement
            var targetTable = columnConfigs.First().Value.TargetTable;
            var updateStatement = new StringBuilder($"update {targetTable} set ");

            foreach (var column in syncEvent.Columns)
            {
                //primary key should not be included
                if (column.Name == primaryKeyConfig.SourceColumn)
                {
                    continue;
                }

                var paramStr = GetSqlParameter(column, columnConfigs[column.Name]);
                updateStatement.Append($"{column.Name} = {paramStr}");

                if (column != syncEvent.Columns.Last())
                {
                    updateStatement.Append(", ");
                }
            }

            updateStatement.Append($" where {primaryKeyConfig.TargetColumn} = {primaryKeyValue}");

            return updateStatement.ToString();
        }

        private string GetSqlParameter(Column column, ColumnSyncConfig columnSyncConfig)
        {
            switch (column.Type)
            {
                case Column.DataType.String:
                    return $"'{column.Value}'";
                case Column.DataType.Int:
                    return $"{column.Value}";
                case Column.DataType.Date:
                    return $"'{column.Value}'";
                default:
                    throw new NotImplementedException($"Column type {column.Type} not implemented.");
            };
        }


        private void ExecuteDBWrite(string sqlStatement)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
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

        private int ExecuteDBQueryCountingRows(string sqlQuery)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
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

