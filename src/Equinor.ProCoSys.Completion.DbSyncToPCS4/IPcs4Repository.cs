using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public interface IPcs4Repository
{
    Task<string> ValueLookupAsync(string sqlQuery, DynamicParameters sqlParameters, CancellationToken cancellationToken);
    Task UpdateSingleRowAsync(string sqlStatement, DynamicParameters sqlParameters, CancellationToken cancellationToken);
}
