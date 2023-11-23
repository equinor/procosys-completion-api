namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public interface IOracleDBExecutor
    {
        Task<int> ExecuteDBQueryCountingRows(string sqlQuery);
        Task<string?> ExecuteDBQueryForValueLookup(string sqlQuery);
        Task ExecuteDBWrite(string sqlStatement);
    }
}
