using System.Text;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.MessageContracts;
using Oracle.ManagedDataAccess.Client;
using static Equinor.ProCoSys.Completion.DbSyncToPCS4.Column;

namespace Equinor.ProCoSys.Completion.DbSyncToPOCS4
{

    public class DbSynchronizer
    {
        public static void SetOracleConnection(string oracleConn) => DbConnStr = oracleConn;

        private static string? DbConnStr { get; set; }

        readonly List<ColumnSyncConfig> _syncMapping = SyncMappingConfig.GetInstance().getSyncMappingList();

        public DbSynchronizer()
        {
            if (DbConnStr == null)
            {
                throw new Exception("DB connection string for Oracle is not set.");
            }
        }

        static public void SyncChangesToMain(object entity, List<IProperty> changes)
        {

            //hent config for  kolonner
            //lag liste med verdier for hver kolonne
            //finn primærnølkkel
            // build sql statement
            //execute

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
            var columnConfigs = _syncMapping.Where(config =>
                    config.SourceTable.Equals(sourceTableName)).ToDictionary(column => column.SourceColumn);

            if (columnConfigs.Count < 0)
            {
                throw new Exception("Map configuraiton is missing.");
            }

            //Create list with changes for columns 
            var columns = new List<Column>();
            foreach (var change in changes)
            {
                var column = new Column() { Name = change.Name, Value = change.NewValue?.ToString(), Type = columnConfigs[change.Name].SourceType };
                columns.Add(column);
            }

            //find primary key using reflection
            var primaryKeyConfig = columnConfigs.Where(config => config.Value.IsPrimaryKey == true).Single().Value;
            var entityType = entity.GetType();
            var primaryKeyInfo = entityType.GetProperty(primaryKeyConfig.SourceType.ToString());

            var primaryKeyValue = "";

            if (primaryKeyInfo != null)
            {
                var verdi = primaryKeyInfo.GetValue(entity);
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

            var primaryKey = new Column() { Name = primaryKeyConfig.SourceColumn, Value = primaryKeyValue, Type = primaryKeyConfig.SourceType };


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
            var insertStatement = BuildInsertStatement(syncEvent);
            Console.WriteLine($"SQL insert statement: {insertStatement}");
            ExecuteDBWrite(insertStatement);
        }

        private void HandleUpdateEvent(SyncEvent syncEvent)
        {
            //todo: kast exception hvis vi mangler config for en kolonne

            var updateStatement = BuildUpdateStatement(syncEvent);
            Console.WriteLine($"SQL update statement: {updateStatement}");
            ExecuteDBWrite(updateStatement);
        }

        private string BuildInsertStatement(SyncEvent syncEvent)
        {


            //DENNE ER IKKE IMPLEMENTERT FERDIG



            //Get config for columns
            var columnConfigs = _syncMapping.Where(config =>
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

                insertStatement.Append($"{column.Value}");

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
            var columnConfigs = _syncMapping.Where(config =>
                    config.SourceTable.Equals(syncEvent.Table.ToLower())).ToDictionary(column => column.SourceColumn);

            if (columnConfigs.Count < 0)
            {
                throw new Exception("Map configuraiton is missing.");
            }

            //find primary key 
            var primaryKeyConfig = columnConfigs.Where(config => config.Value.IsPrimaryKey == true).Single().Value;

            //Verify that one and only one row exist in target db, for given primarykey
            var noOfRowsInTargetQuery = $"select count(*) from {primaryKeyConfig.TargetTable} where {primaryKeyConfig.TargetColumn} = {getSqlParameter(syncEvent.PrimaryKey.Value, syncEvent.PrimaryKey.Type)}";

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
                //primary key must not be included (da må det være en bug et sted...)
                if (column.Name == primaryKeyConfig.SourceColumn)
                {
                    continue;
                }

                var value = GetValueForColumn(column, columnConfigs[column.Name]);

                updateStatement.Append($"{column.Name} = {value}");

                if (column != syncEvent.Columns.Last())
                {
                    updateStatement.Append(", ");
                }
            }

            updateStatement.Append($" where {primaryKeyConfig.TargetColumn} = {getSqlParameter(syncEvent.PrimaryKey.Value, syncEvent.PrimaryKey.Type)}");

            return updateStatement.ToString();
        }

        private string GetValueForColumn(Column column, ColumnSyncConfig columnConfig)
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


        public string getSqlParameter(string? value, DataType type)
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


        private void ExecuteDBWrite(string sqlStatement)
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

        private int ExecuteDBQueryCountingRows(string sqlQuery)
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

