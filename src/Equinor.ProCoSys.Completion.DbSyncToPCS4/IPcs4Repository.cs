using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public interface IPcs4Repository
{
    Task ExecuteRowOperationAsync(string sqlStatement, DynamicParameters sqlParameters, CancellationToken cancellationToken);
    Task<long> ValueLookupNumberAsync(string sqlQuery, DynamicParameters sqlParameters, CancellationToken cancellationToken);
}
