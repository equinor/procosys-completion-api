﻿using Equinor.ProCoSys.Completion.DbSyncToPCS4.MappingConfig;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * This class provide an interface to the operations for synchronizing data to ProCoSys 4. 
 * The class should only be used as a singleton (through dependency injection). 
 * It will hold a list with mapping configuration. The mapping configuration given the mapping 
 * of properties in a source object to columns in a table in the PCS 4 database. 
 */
public class SyncToPCS4Service : ISyncToPCS4Service
{
    readonly IPcs4Repository _pcs4Repository;

    public SyncToPCS4Service(IPcs4Repository oracleDBExecutor) => _pcs4Repository = oracleDBExecutor;

    /**
     * Updates the PCS 4 database with changes provided in the sourceObject, 
     * given the mapping configuration. 
     */
    public async Task SyncUpdatesAsync(string sourceObjectName, object sourceObject, CancellationToken cancellationToken = default)
    {
        var sourceObjectMappingConfig = GetMappingConfigurationForSourceObject(sourceObjectName);

        var syncUpdateHandler = new SyncUpdateHandler(_pcs4Repository);
        var (sqlUpdateStatement, sqlParameters) = await syncUpdateHandler.BuildSqlUpdateStatementAsync(sourceObjectMappingConfig, sourceObject, cancellationToken);

        await _pcs4Repository.UpdateSingleRowAsync(sqlUpdateStatement, sqlParameters, cancellationToken);
    }

    /**
     * Will return the mapping configuration for the given source object
     */
    private static ISourceObjectMappingConfig GetMappingConfigurationForSourceObject(string sourceObjectName)
    {
        return sourceObjectName switch
        {
            "PunchItem" => new PunchItemMappingConfig(),
            _ => throw new NotImplementedException($"Mapping is not implemented for source object with name '{sourceObjectName}'."),
        };
    }
}

