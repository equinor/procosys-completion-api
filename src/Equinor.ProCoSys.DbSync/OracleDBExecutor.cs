using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    /**
     * Contains methods to execute queries and updates on the Oracle database for PCS 4. 
     */
    public class OracleDBExecutor : IOracleDBExecutor
    {
        private readonly string _dbConnStr;

        public OracleDBExecutor(IOptionsMonitor<OracleDBConnectionOptions> options) => _dbConnStr = options.CurrentValue.ConnectionString;

        public async Task ExecuteDBWrite(string sqlStatement)
        {
            if (_dbConnStr == null)
            {
                throw new Exception("DB connection string for Oracle is not set.");
            }

            using var connection = new OracleConnection(_dbConnStr);
            connection.Open();

            using var cmd = new OracleCommand(sqlStatement, connection);
            //todo: Bør bruke Parameters.Add, for unngå sql injection
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"{rowsAffected} rows affected.");
        }

        public async Task<int> ExecuteDBQueryCountingRows(string sqlQuery)
        {
            if (_dbConnStr == null)
            {
                throw new Exception("DB connection string for Oracle is not set.");
            }

            using var connection = new OracleConnection(_dbConnStr);
            connection.Open();

            using var command = new OracleCommand(sqlQuery, connection);
            //todo: Bør bruke Parameters.Add, for unngå sql injection
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
            return -1;
        }

        public async Task<string?> ExecuteDBQueryForValueLookup(string sqlQuery)
        {
            using var connection = new OracleConnection(_dbConnStr);
            connection.Open();

            using var command = new OracleCommand(sqlQuery, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                //todo: Bør kanskje sjekke at vi bare for returnert én rad
                var value = Convert.ToString(await command.ExecuteScalarAsync());
                if (value != null)
                {
                    return value;
                }
            }
            return null;
        }
    }
}
