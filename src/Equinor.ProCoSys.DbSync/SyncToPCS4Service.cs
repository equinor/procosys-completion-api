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
        readonly SyncMappingConfig _syncMappingConfig;
        readonly IOracleDBExecutor _oracleDBExecutor;

        public SyncToPCS4Service(IOracleDBExecutor oracleDBExecutor)
        {
            _syncMappingConfig = new SyncMappingConfig();
            _oracleDBExecutor = oracleDBExecutor;
        }

        /**
         * Updates the PCS 4 database with changes provided in the sourceObject, 
         * given the mapping configuration. 
         */
        public async Task SyncUpdates(string sourceObjectName, object sourceObject, CancellationToken token = default)
        {
            var syncUpdateHandler = new SyncUpdateHandler(_oracleDBExecutor);
            await syncUpdateHandler.HandleAsync(sourceObject, _syncMappingConfig, token);
        }
    }
}

