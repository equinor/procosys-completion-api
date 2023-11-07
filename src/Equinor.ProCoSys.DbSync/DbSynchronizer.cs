using System.Configuration;
using System.Reflection;
using System.Text;
using Equinor.ProCoSys.Completion.MessageContracts;
using Oracle.ManagedDataAccess.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Equinor.ProCoSys.DbSyncPOC.Column;

namespace Equinor.ProCoSys.DbSyncPOC
{

    public class DbSynchronizer
    {
        public static void SetOracleConnection(string oracleConn)
        {
            connectionString = oracleConn;
        }

        private static string? connectionString { get; set; }

        List<ColumnSyncConfig> _syncConfig = new List<ColumnSyncConfig>();

        public DbSynchronizer()
        {
            //Read sync configuration from file 
            string filePath = "C:/procosys/SyncConfig_test.csv";

            string[] lines = File.ReadAllLines(filePath);
            for (var i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(',');

                DataType type = (DataType)Enum.Parse(typeof(DataType), columns[5]);

                var config = new ColumnSyncConfig() { SourceTable = columns[0], TargetTable = columns[1], SourceColumn = columns[2], TargetColumn = columns[3], IsPrimaryKey = bool.Parse(columns[4]), Type = type };
                _syncConfig.Add(config);
            }
        }

        static public void SyncChangesToMain(object entity, List<IProperty> changes)
        {
            if (changes.Any())
            {
                var dbSynchronizer = new DbSynchronizer();

                var syncEvent = dbSynchronizer.BuildSyncEventForUpdate(entity, changes);


                dbSynchronizer.HandleEvent(syncEvent);
            }
        }

        private SyncEvent BuildSyncEventForUpdate(object entity, List<IProperty> changes)
        {
            var sourceTableName = entity.GetType().Name.ToLower();

            //Get config for columns
            var columnConfigs = _syncConfig.Where(config =>
                    config.SourceTable.Equals(sourceTableName)).ToDictionary(column => column.SourceColumn);

            if (columnConfigs.Count < 0)
            {
                throw new Exception("Map configuraiton is missing.");
            }


            //Create list with changes for columns 
            var columns = new List<Column>();
            foreach (var change in changes)
            {
                var column = new Column() { Name = change.Name, Value = change.NewValue?.ToString(), Type = columnConfigs[change.Name].Type };
                columns.Add(column);
            }


            //find primary key using reflection
            var primaryKeyConfig = columnConfigs.Where(config => config.Value.IsPrimaryKey == true).Single().Value;
            Type entityType = entity.GetType();
            PropertyInfo primaryKeyInfo = entityType.GetProperty("Guid");

            string primaryKeyValue = "";

            if (primaryKeyInfo != null)
            {
                object verdi = primaryKeyInfo.GetValue(entity);
                if (verdi != null)
                {
                    primaryKeyValue = verdi.ToString();
                }
                else
                {
                    //kast exception
                }
            }
            else
            {
                //kast exception
            }

            var primaryKey = new Column() { Name = primaryKeyConfig.SourceColumn, Value = primaryKeyValue, Type = primaryKeyConfig.Type };


            //Create sync event 
            var syncEvent = new SyncEvent()
            {
                Operation = SyncEvent.OperationType.Update,
                Table = sourceTableName,
                Columns = columns,
                PrimaryKey = primaryKey
            };

            return syncEvent;
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

                insertStatement.Append($"{column.getSqlParameter()}");

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

            //Verify that one and only one row exist in target db, for given primarykey
            var noOfRowsInTargetQuery = $"select count(*) from {primaryKeyConfig.TargetTable} where {primaryKeyConfig.TargetColumn} = {syncEvent.PrimaryKey.getSqlParameter()}";

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

                updateStatement.Append($"{column.Name} = {column.getSqlParameter()}");

                if (column != syncEvent.Columns.Last())
                {
                    updateStatement.Append(", ");
                }
            }

            updateStatement.Append($" where {primaryKeyConfig.TargetColumn} = {syncEvent.PrimaryKey.getSqlParameter()}");

            return updateStatement.ToString();
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

