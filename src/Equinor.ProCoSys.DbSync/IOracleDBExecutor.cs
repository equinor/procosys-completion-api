using Oracle.ManagedDataAccess.Client;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public interface IOracleDBExecutor
    {
        Task<string?> ExecuteDBQueryForValueLookupAsync(string sqlQuery, CancellationToken cancellationToken);
        Task ExecuteDBWriteAsync(string sqlStatement, CancellationToken cancellationToken);

    }
}
