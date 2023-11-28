using System.Data;
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

        public OracleDBExecutor(IOptionsMonitor<OracleDBConnectionOptions> options)
        {
            _dbConnStr = options.CurrentValue.ConnectionString;

            if (_dbConnStr == null)
            {
                throw new Exception("DB connection string for Oracle is not set.");
            }
        }

        public async Task ExecuteDBWriteAsync(string sqlStatement, CancellationToken cancellationToken)
        {
            using var connection = new OracleConnection(_dbConnStr);
            await connection.OpenAsync(cancellationToken);
            using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = sqlStatement;
                //todo: Bør bruke Parameters.Add, for unngå sql injection
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                if (rowsAffected != 1)
                {
                    throw new Exception($"Sql statement affected {rowsAffected} rows. Should only affect one row.");
                }
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                //todo: bør vi logge noe her? 
                throw;
            }
        }

        public async Task<string?> ExecuteDBQueryForValueLookupAsync(string sqlQuery, CancellationToken cancellationToken)
        {
            using var connection = new OracleConnection(_dbConnStr);
            await connection.OpenAsync(cancellationToken);
            using var command = new OracleCommand(sqlQuery, connection);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (reader.Read())
            {
                //todo: Bør kanskje sjekke at vi bare for returnert én rad
                var value = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken));
                if (value != null)
                {
                    return value;
                }
            }
            return null;
        }
    }
}
