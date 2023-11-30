using System.Threading;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    /**
     * This class provide an interface to the operations for synchronizing data to ProCoSys 4. 
     * The class should only be used as a singleton (through dependency injection). 
     * It will hold a list with mapping configuration. The mapping configuration given the mapping 
     * of properties in a source object to columns in a table in the PCS 4 database. 
     */
    public class SyncToPCS4Service : ISyncToPCS4Service
    {
        readonly IOracleDBExecutor _oracleDBExecutor;
        readonly ISyncMappingConfig _syncMappingConfig;


        public SyncToPCS4Service(IOracleDBExecutor oracleDBExecutor, ISyncMappingConfig syncMappingConfig)
        {
            _syncMappingConfig = syncMappingConfig;
            _oracleDBExecutor = oracleDBExecutor;
        }

        /**
         * Updates the PCS 4 database with changes provided in the sourceObject, 
         * given the mapping configuration. 
         */
        public async Task SyncUpdatesAsync(string sourceObjectName, object sourceObject, CancellationToken cancellationToken = default)
        {
            var syncUpdateHandler = new SyncUpdateHandler(_oracleDBExecutor);

            var sqlUpdateStatement = await syncUpdateHandler.BuildSqlUpdateStatementAsync(sourceObjectName, sourceObject, _syncMappingConfig, cancellationToken);
            await _oracleDBExecutor.ExecuteDBWriteAsync(sqlUpdateStatement, cancellationToken);

        }
    }
}

