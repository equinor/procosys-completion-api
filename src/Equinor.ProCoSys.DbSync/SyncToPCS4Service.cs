namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    /**
     * This class provide an interface to the operations for synchronizing data to ProCoSys 4. 
     * The class should only be used as a singleton (through dependency injection). It will hold a list with mapping configuration.  
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
         * Update the PCS 4 database with all changes provided in the entity class.  
         */
        public async Task SyncUpdates(object entity, CancellationToken token = default)
        {
            var syncUpdateHandler = new SyncUpdateHandler(_oracleDBExecutor);
            await syncUpdateHandler.HandleAsync(entity, _syncMappingConfig, token);
        }
    }
}

