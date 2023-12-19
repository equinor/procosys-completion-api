using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Contains methods to execute queries and updates on the Oracle database for PCS 4. 
 */
public class Pcs4Repository : IPcs4Repository
{
    private readonly string _dbConnStr;

    public Pcs4Repository(IOptionsMonitor<OracleDBConnectionOptions> options)
    {
        _dbConnStr = options.CurrentValue.ConnectionString;

        if (_dbConnStr == null)
        {
            throw new Exception("DB connection string for Oracle is not set.");
        }
    }

    /**
     * This method will perform a update on the Pcs4 database, using the given sqlStatment.
     * If more than one row is affected by the sql statment, an exception will be thrown, and the udpate will not be commited. 
     */
    public async Task UpdateSingleRowAsync(string sqlStatement, DynamicParameters sqlParameters, CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(_dbConnStr);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            var rowsAffected = await connection.ExecuteAsync(sqlStatement, sqlParameters, transaction);

            if (rowsAffected != 1)
            {
                throw new Exception($"Expected one and only one row update for the sql statement: '{sqlStatement}', " +
                    $"but got {rowsAffected}. Update is not commited.");
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new Exception($"Error occured when trying to execute the following sql statement: {sqlStatement}. A rollback on the transaction is performed.");
        }
    }

    /**
     * This method will be used to fetch a value from Pcs4 datbase, by 
     */
    public async Task<string?> ValueLookupAsync(string sqlQuery, CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(_dbConnStr);
        await connection.OpenAsync(cancellationToken);

        //DAPPER
        await using var command = new OracleCommand(sqlQuery, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

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
